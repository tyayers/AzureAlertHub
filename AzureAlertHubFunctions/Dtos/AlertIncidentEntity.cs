using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureAlertHubFunctions.Dtos
{
    public class AlertIncidentEntity : TableEntity
    {
        public AlertIncidentEntity(string incidentSystem, string incidentId)
            : base(incidentSystem, incidentId)
        {
            IncidentSystem = incidentSystem;
            IncidentId = incidentId;
        }

        public AlertIncidentEntity() { }
    
        public string IncidentSystem { get; set; }
        public string IncidentId { get; set; }
        public string IncidentUrl { get; set; }
        public string AlertPartitionId { get; set; }
        public string AlertRowId { get; set; }
        public bool Confirmed { get; set; } = false;
    }
}
