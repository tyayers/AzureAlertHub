using AzureAlertHubFunctions.Dtos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureAlertHubFunctions.Interfaces
{
    public interface IAlertUtilities
    {
        AlertEntity[] LoadOrCreateAlerts(string payload, ILogger log);
        AlertEntity GetAlert(string partitionKey, string rowKey);
        void CheckAlertsStatus(ILogger log);
        void CheckIncidentsStatus(ILogger log);
        AlertIncidentEntity GetAlertIncident(string partitionKey, string rowKey);
        List<AlertEntity> GetAllAlerts();
        List<AlertIncidentEntity> GetAllAlertIncidents();
        void InsertUpdateAlert(AlertEntity alert);
        void DeleteAlert(AlertEntity alert);
        void DeleteAlertIncident(AlertIncidentEntity alert);
        
    }
}
