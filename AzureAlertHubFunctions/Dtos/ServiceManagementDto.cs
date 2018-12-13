using System;
using System.Collections.Generic;
using System.Text;

namespace AzureAlertHubFunctions.Dtos
{
    public class ServiceManagementDto
    {
        public string caller_id { get; set; }
        public string opened_by { get; set; }
        public string business_service { get; set; }
        public string it_service { get; set; }
        public string contact_type { get; set; }
        public string short_description { get; set; }
        public string description { get; set; }
        public string assignment_group { get; set; }
        public string group_family { get; set; }
        public string location { get; set; }
        public string gravity { get; set; }
        public string impact { get; set; }
        public string stage { get; set; }
    }
}
