using AzureAlertHubFunctions.Dtos;
using AzureAlertHubFunctions.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureAlertHubFunctions.Services
{
    public class TESTManagement : IServiceManagement
    {
        public ServiceManagementResponseDto CreateIncident(AlertEntity alert)
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
    }
}
