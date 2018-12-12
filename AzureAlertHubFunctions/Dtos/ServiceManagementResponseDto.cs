using System;
using System.Collections.Generic;
using System.Text;

namespace AzureAlertHubFunctions.Dtos
{
    public class ServiceManagementResponseDto
    {
        public ServiceManagementResponse result { get; set; }
    }

    public class ServiceManagementResponse
    {
        public string number { get; set; }
        public string url { get; set; }
    }
}
