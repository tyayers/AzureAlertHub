using System;
using System.Collections.Generic;
using System.Text;

namespace AzureAlertHubFunctions.Dtos
{
    public class AlertResult
    {
        public string ResourceName { get; set; }
        public string InstanceName { get; set; } = "";
        public string PartitionKey { get; set; }
        public string Description { get; set; } = "";
        public string Type { get; set; } = "OTHER";
    }
}
