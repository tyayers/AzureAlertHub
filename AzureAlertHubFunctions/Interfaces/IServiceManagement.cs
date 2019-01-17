using AzureAlertHubFunctions.Dtos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureAlertHubFunctions.Interfaces
{
    public interface IServiceManagement
    {
        ServiceManagementResponseDto CreateIncident(Dtos.AlertEntity alert, ILogger log);

        ServiceManagementStatusResponseDto GetIncidentStatus(Dtos.AlertIncidentEntity incident, ILogger log);

        ServiceManagementStatusResponseDto GetIncidentsStatus(List<Dtos.AlertIncidentEntity> incidents, ILogger log);
    }
}
