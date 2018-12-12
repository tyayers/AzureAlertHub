using AzureAlertHubFunctions.Dtos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureAlertHubFunctions.Interfaces
{
    public interface IAlertUtilities
    {
        AlertEntity LoadOrCreateAlert(string payload, ILogger log);
        AlertEntity RetrieveAlert(string partitionKey, string rowKey);
        AlertIncidentEntity RetrieveAlertEntity(string partitionKey, string rowKey);
        void InsertUpdateAlert(AlertEntity alert);
        void DeleteAlert(AlertEntity alert);
        void DeleteAlertIncident(AlertIncidentEntity alert);
        string GetResourceName(string alertName, Newtonsoft.Json.Linq.JObject payload, ILogger log);
        string GetClientInstance(string alertName, string resourceName, string payload, ILogger log);
    }
}
