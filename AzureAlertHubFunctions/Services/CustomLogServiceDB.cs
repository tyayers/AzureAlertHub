using AzureAlertHubFunctions.Dtos;
using AzureAlertHubFunctions.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.RegularExpressions;

namespace AzureAlertHubFunctions.Services
{
    public class CustomLogServiceDB : ICustomLogHandler
    {
        protected string regex;
        protected string type;
        public CustomLogServiceDB(string Regex, string Type)
        {
            regex = Regex;
            type = Type;
        }

        public string LogType
        {
            get
            {
                return type;
            }
        }

        public CustomLogHanderResultDto CheckCustomLog(string AlertName, string HostName, string InstanceName, string Description, JObject table, JArray row, string rowPayload, ILogger log)
        {
            CustomLogHanderResultDto result = new CustomLogHanderResultDto();

            string resourceHostName = HostName;
            if (resourceHostName.Contains("."))
            {
                resourceHostName = resourceHostName.Substring(0, resourceHostName.IndexOf('.') - 1);
            }

            string regexString = regex;
            regexString = regexString.Replace("{HOSTNAME}", resourceHostName);

            log.LogInformation($"DB regex - check for alert {AlertName}: {regexString} on {rowPayload}");
            // Check if body contains a database, then report incident
            Regex rx = new Regex(@regexString, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            // Find matches.
            MatchCollection matches = rx.Matches(rowPayload);

            if (matches.Count > 0)
            {
                result.Handled = true;

                // If we have a match with databases
                foreach (Match match in matches)
                {
                    log.LogInformation($"DB regex - found match for alert {AlertName} and regex {regexString}: {match.Value}!");

                    AlertResult dbResult = new AlertResult() { ResourceName = HostName, InstanceName = match.Value.Replace($"{resourceHostName}\\", "") };
                    dbResult.PartitionKey = dbResult.ResourceName + " - " + dbResult.InstanceName;
                    dbResult.Type = type;
                    dbResult.Description = Description;
                    result.Results.Add(dbResult);
                }
            }
            else
            {
                log.LogInformation($"DB regex - No matches found!");
            }

            return result;
        }
    }
}
