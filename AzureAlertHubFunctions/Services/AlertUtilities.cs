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
        public AlertEntity LoadOrCreateAlert(string payload, ILogger log)
        {
            AlertEntity alert = null;

            Newtonsoft.Json.Linq.JObject obj = Newtonsoft.Json.Linq.JObject.Parse(payload);
            if (obj != null && obj["data"] != null)
            {
                string AlertRuleName = "NO-NAME-FOUND";
                if (obj["data"]["AlertRuleName"] != null) AlertRuleName = obj["data"]["AlertRuleName"].ToString();

                string LogAnalyticsUrl = "";
                if (obj["data"]["LinkToSearchResults"] != null) LogAnalyticsUrl = obj["data"]["LinkToSearchResults"].ToString();
                string ResourceName = GetResourceName(AlertRuleName, obj, log);
                string ClientInstance = GetClientInstance(AlertRuleName, ResourceName, payload, log);

                // Add computer to AlertRuleName 
                if (!String.IsNullOrEmpty(ClientInstance) && !String.IsNullOrEmpty(AlertRuleName))
                {
                    alert = RetrieveAlert(ClientInstance, AlertRuleName);
                    if (alert == null)
                    {
                        alert = new AlertEntity(ClientInstance, AlertRuleName);
                        alert.Payload = payload;
                        alert.SearchIntervalStartTimeUtc = DateTime.Parse(obj["data"]["SearchIntervalStartTimeUtc"].ToString());
                        alert.SearchIntervalEndTimeUtc = DateTime.Parse(obj["data"]["SearchIntervalEndtimeUtc"].ToString());
                        alert.LogAnalyticsUrl = LogAnalyticsUrl;
                        alert.Resource = ResourceName;
                    }
                    else
                    {
                        alert.LastOccuranceTimestamp = DateTime.Now;
                        alert.Counter++;
                    }

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

            return alert;
        }

        public string GetResourceName(string alertName, Newtonsoft.Json.Linq.JObject payload, ILogger log)
        {
            string resourceName = "";

            try
            {
                resourceName = payload["data"]["SearchResult"]["tables"][0]["rows"][0][1].ToString();
            }
            catch (Exception ex)
            {
                log.LogError($"Error retrieving resource name for alert {alertName} - {ex.ToString()}");                
            }

            return resourceName;
        }

        public string GetClientInstance(string alertName, string resourceName, string payload, ILogger log)
        {
            string clientInstance = resourceName;

            if (resourceName.Contains("."))
            {
                resourceName = resourceName.Substring(0, resourceName.IndexOf('.') - 1);
            }

            // Define a regular expression for repeated words.
            Regex rx = new Regex($@"({resourceName}\\)\w+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            // Find matches.
            MatchCollection matches = rx.Matches(payload);

            // Report on each match.
            foreach (Match match in matches)
            {
                clientInstance = match.Value;
            }

            return clientInstance.Replace("\\", "-");
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
