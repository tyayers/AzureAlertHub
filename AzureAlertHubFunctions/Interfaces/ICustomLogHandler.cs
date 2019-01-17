using AzureAlertHubFunctions.Dtos;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureAlertHubFunctions.Interfaces
{
    public interface ICustomLogHandler
    {
        CustomLogHanderResultDto CheckCustomLog(string HostName, string InstanceName, string Description, JObject table, JArray row, string rowPayload, ILogger log);
    }
}
