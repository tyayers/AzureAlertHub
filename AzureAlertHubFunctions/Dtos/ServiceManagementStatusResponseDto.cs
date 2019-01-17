using System;
using System.Collections.Generic;
using System.Text;

namespace AzureAlertHubFunctions.Dtos
{
    public class ServiceManagementStatusResponseDto
    {
        public Dictionary<string, ServiceManagementStatus> result { get; set; } = new Dictionary<string, ServiceManagementStatus>();
    }

    public class ServiceManagementStatus
    {
        public string state { get; set; }
        public string short_description { get; set; }
    }
}
