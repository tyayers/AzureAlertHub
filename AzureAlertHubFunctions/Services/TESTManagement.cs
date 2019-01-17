using AzureAlertHubFunctions.Dtos;
using AzureAlertHubFunctions.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureAlertHubFunctions.Services
{
    public class TESTManagement : IServiceManagement
    {
        public ServiceManagementResponseDto CreateIncident(AlertEntity alert, ILogger log)
        {
            string number = System.DateTime.Now.Ticks.ToString();
            ServiceManagementResponseDto result = new ServiceManagementResponseDto();
            result.result = new ServiceManagementResponse()
            {
                number = number,
                url = $"https://yyy.servicenow.com/yyyyyy/yyy?incident={number}"
            };

            return result;
        }

        public ServiceManagementStatusResponseDto GetIncidentStatus(AlertIncidentEntity incident, ILogger log)
        {
            ServiceManagementStatusResponseDto result = new ServiceManagementStatusResponseDto();
            result.result.Add(incident.IncidentId, new ServiceManagementStatus() { state = "Closed", short_description = "Nothing to say.." });
            return result;
        }

        public ServiceManagementStatusResponseDto GetIncidentsStatus(List<AlertIncidentEntity> incidents, ILogger log)
        {
            ServiceManagementStatusResponseDto result = new ServiceManagementStatusResponseDto();

            foreach (AlertIncidentEntity incident in incidents)
            {
                result.result.Add(incident.IncidentId, new ServiceManagementStatus() { state = "Closed", short_description = "Nothing to say.." });
            }

            return result;
        }
    }
}
