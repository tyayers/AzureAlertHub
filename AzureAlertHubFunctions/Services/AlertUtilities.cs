using AzureAlertHubFunctions.Dtos;
using AzureAlertHubFunctions.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AzureAlertHubFunctions.Services
{
    public class AlertUtilities : IAlertUtilities
    {
        protected IServiceManagement serviceManagemement;
        public AlertUtilities(IServiceManagement serviceManagemementAdapter)
        {
            serviceManagemement = serviceManagemementAdapter;
        }
        /// <summary>
        /// The GetAlertParameters method returns the SubscriptionId and AlertRuleName if they are available in the payload
        /// </summary>
        /// <param name="payload">The JSON payload from Azure Alerting</param>
        /// <returns>AlertParameters object with SubscriptionId and AlertRuleName</returns>
        public AlertEntity[] LoadOrCreateAlerts(string payload, ILogger log)
        {
            List<AlertEntity> results = new List<AlertEntity>();

            Newtonsoft.Json.Linq.JObject obj = Newtonsoft.Json.Linq.JObject.Parse(payload);
            if (obj != null && obj["data"] != null)
            {
                string AlertRuleName = "NO-NAME-FOUND";
                if (obj["data"]["AlertRuleName"] != null) AlertRuleName = obj["data"]["AlertRuleName"].ToString();

                string LogAnalyticsUrl = "";
                if (obj["data"]["LinkToSearchResults"] != null) LogAnalyticsUrl = obj["data"]["LinkToSearchResults"].ToString();
                AlertResult[] alertResults = GetAlertResults(AlertRuleName, payload, obj, log);

                foreach (AlertResult result in alertResults)
                {
                    // Add computer to AlertRuleName 
                    if (!String.IsNullOrEmpty(result.PartitionKey) && !String.IsNullOrEmpty(AlertRuleName))
                    {
                        AlertEntity alert = RetrieveAlert(result.PartitionKey, AlertRuleName);
                        if (alert == null)
                        {
                            alert = new AlertEntity(result.PartitionKey, AlertRuleName);
                            alert.Payload = payload;
                            alert.SearchIntervalStartTimeUtc = DateTime.Parse(obj["data"]["SearchIntervalStartTimeUtc"].ToString());
                            alert.SearchIntervalEndTimeUtc = DateTime.Parse(obj["data"]["SearchIntervalEndtimeUtc"].ToString());
                            alert.LogAnalyticsUrl = LogAnalyticsUrl;
                            alert.Resource = result.ResourceName;
                            alert.ClientInstance = result.InstanceName;
                        }
                        else
                        {
                            alert.LastOccuranceTimestamp = DateTime.Now;
                            alert.Counter++;
                        }

                        results.Add(alert);

                        if (String.IsNullOrEmpty(alert.IncidentId))
                        {
                            // We don't yet have an IncidentId for this alert
                            ServiceManagementResponseDto incident = serviceManagemement.CreateIncident(alert, log);
                            if (incident != null && incident.result != null)
                            {
                                alert.IncidentId = incident.result.number;
                                alert.IncidentUrl = incident.result.url;
                            }
                        }

                        log.LogInformation("Update Alerts table: " + Newtonsoft.Json.JsonConvert.SerializeObject(alert));
                        InsertUpdateAlert(alert);

                        if (!String.IsNullOrEmpty(alert.IncidentId))
                        {
                            // Insert record of incident ID
                            AlertIncidentEntity incidentEntity = new AlertIncidentEntity("SNOW", alert.IncidentId);
                            incidentEntity.AlertPartitionId = alert.PartitionKey;
                            incidentEntity.AlertRowId = alert.RowKey;
                            incidentEntity.IncidentUrl = alert.IncidentUrl;

                            InsertUpdateAlertIncident(incidentEntity);
                        }
                    }
                }
            }

            return results.ToArray();
        }

        protected AlertResult[] GetAlertResults(string alertName, string payload, Newtonsoft.Json.Linq.JObject payloadObj, ILogger log)
        {
            List<AlertResult> results = new List<AlertResult>();

            foreach (JObject table in payloadObj["data"]["SearchResult"]["tables"])
            {
                int resourceIndex = GetColumnIndex(alertName, "Computer", table, log);
                int instanceIndex = GetColumnIndex(alertName, "InstanceName", table, log);

                if (resourceIndex != -1)
                {
                    foreach (JArray row in table["rows"])
                    {
                        AlertResult result = new AlertResult() { ResourceName = row[resourceIndex].ToString(), PartitionKey = row[resourceIndex].ToString() };

                        if (instanceIndex != -1 && row[instanceIndex].ToString() != "")
                        {
                            result.InstanceName = row[instanceIndex].ToString();
                            result.PartitionKey = result.ResourceName + " - " + result.InstanceName;
                        }

                        // Get host name without domain
                        string resourceHostName = result.ResourceName;
                        if (resourceHostName.Contains("."))
                        {
                            resourceHostName = resourceHostName.Substring(0, resourceHostName.IndexOf('.') - 1);
                        }

                        // Check if body contains a database, then report incident
                        Regex rx = new Regex($@"({resourceHostName}\\)\w+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

                        // Find matches.
                        MatchCollection matches = rx.Matches(payload);

                        if (matches.Count > 0)
                        {
                            // If we have a match with databases
                            foreach (Match match in matches)
                            {
                                AlertResult dbResult = new AlertResult() { ResourceName = result.ResourceName, InstanceName = match.Value.Replace("\\", "-") };
                                dbResult.PartitionKey = dbResult.ResourceName + " - " + dbResult.InstanceName;
                                dbResult.Type = AlertType.DB;
                                results.Add(dbResult);
                            }
                        }
                        else
                        {
                            // If no databases, then add original incident (either CPU, DISK, etc..)
                            if (result.InstanceName.Contains(":")) result.Type = AlertType.DISK;
                            results.Add(result);
                        }
                    }
                }
            }

            return results.ToArray();
        }

        protected int GetColumnIndex(string alertName, string columnName, Newtonsoft.Json.Linq.JObject tableObject, ILogger log)
        {
            int ResourceIndex = -1;

            for (int p = 0; p < ((JArray)tableObject["columns"]).Count; p++)
            {
                if (tableObject["columns"][p]["name"].ToString() == columnName)
                {
                    ResourceIndex = p;
                    break;
                }
            }

            return ResourceIndex;
        }

        public AlertEntity RetrieveAlert(string partitionKey, string rowKey)
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(System.Environment.GetEnvironmentVariable("StorageConnectionString"));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference("Alerts");
            table.CreateIfNotExistsAsync().Wait();

            TableOperation tableOperation = TableOperation.Retrieve<AlertEntity>(partitionKey, rowKey);
            TableResult tableResult = table.ExecuteAsync(tableOperation).Result;

            return (AlertEntity) tableResult.Result;
        }

        public AlertIncidentEntity RetrieveAlertEntity(string partitionKey, string rowKey)
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(System.Environment.GetEnvironmentVariable("StorageConnectionString"));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference("AlertIncidents");
            table.CreateIfNotExistsAsync().Wait();

            TableOperation tableOperation = TableOperation.Retrieve<AlertIncidentEntity>(partitionKey, rowKey);
            TableResult tableResult = table.ExecuteAsync(tableOperation).Result;

            return (AlertIncidentEntity)tableResult.Result;
        }

        public void InsertUpdateAlert(AlertEntity alert)
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(System.Environment.GetEnvironmentVariable("StorageConnectionString"));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference("Alerts");
            table.CreateIfNotExistsAsync().Wait();

            // Create the TableOperation object that inserts the customer entity.
            TableOperation insertOperation = TableOperation.InsertOrReplace(alert);

            // Execute the insert operation.
            table.ExecuteAsync(insertOperation);
        }

        public void InsertUpdateAlertIncident(AlertIncidentEntity alert)
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(System.Environment.GetEnvironmentVariable("StorageConnectionString"));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference("AlertIncidents");
            table.CreateIfNotExistsAsync().Wait();

            // Create the TableOperation object that inserts the customer entity.
            TableOperation insertOperation = TableOperation.InsertOrReplace(alert);

            // Execute the insert operation.
            table.ExecuteAsync(insertOperation);
        }

        public void DeleteAlert(AlertEntity alert)
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(System.Environment.GetEnvironmentVariable("StorageConnectionString"));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference("Alerts");
            table.CreateIfNotExistsAsync().Wait();

            // Create the TableOperation object that inserts the customer entity.
            TableOperation deleteOperation = TableOperation.Delete(alert);

            // Execute the insert operation.
            table.ExecuteAsync(deleteOperation);
        }

        public void DeleteAlertIncident(AlertIncidentEntity alert)
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(System.Environment.GetEnvironmentVariable("StorageConnectionString"));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference("AlertIncidents");
            table.CreateIfNotExistsAsync().Wait();

            // Create the TableOperation object that inserts the customer entity.
            TableOperation deleteOperation = TableOperation.Delete(alert);

            // Execute the insert operation.
            table.ExecuteAsync(deleteOperation);
        }
    }
}
