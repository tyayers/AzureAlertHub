using AzureAlertHubFunctions.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureAlertHubFunctions.Interfaces
{
    public interface IServiceManagement
    {
        ServiceManagementResponseDto CreateIncident(Dtos.AlertEntity alert);
    }
}
