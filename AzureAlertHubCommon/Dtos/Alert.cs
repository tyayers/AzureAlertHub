using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureAlertHubCommon.Dtos
{
    public class AlertEntity : TableEntity
    {
        public AlertEntity(string subsciptionId, string alertRuleName)
            : base(subsciptionId, alertRuleName) { }

        public DateTime? SearchIntervalStartTimeUtc { get; set; }
        public DateTime? SearchIntervalEndtimeUtc { get; set; }
        public string AlertPayload { get; set; }
    }
}
