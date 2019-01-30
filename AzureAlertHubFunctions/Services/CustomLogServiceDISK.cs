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
    public class CustomLogServiceDISK : ICustomLogHandler
    {
        protected string regex;
        protected string type;
        public CustomLogServiceDISK(string Regex, string Type)
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

            try
            {
                // Check if body contains disk info, then report incident
                Regex rx = new Regex(@regex, RegexOptions.Compiled | RegexOptions.IgnoreCase);

                // Find matches in the instance text.
                MatchCollection matches = rx.Matches(InstanceName);

                if (matches.Count > 0)
                {
                    result.Handled = true;

                    // If we have a match with disks
                    foreach (Match match in matches)
                    {
                        bool createIncident = true;

                        int freeSpacePercentIndex = GetColumnIndex("Free_Space_Percent", table);
                        int freeMegabytesIndex = GetColumnIndex("Free_MB", table);
                        decimal freeSpacePercent = 0, freeMegabytes = 0, diskSize = 0;

                        if (freeSpacePercentIndex != -1)
                        {
                            freeSpacePercent = Math.Round(Convert.ToDecimal(row[freeSpacePercentIndex].ToString()));
                        }

                        if (freeMegabytesIndex != -1)
                        {
                            freeMegabytes = Convert.ToDecimal(row[freeMegabytesIndex].ToString());
                        }

                        if (freeSpacePercent > 0 && freeMegabytes > 0)
                        {
                            diskSize = Math.Round(freeMegabytes / (freeSpacePercent / 100));
                        }

                        if (diskSize > 500000 && freeSpacePercent > 5)
                        {
                            // Don't create incident if disk is large (> 500 GB) and free space percent is greater than 5
                            createIncident = false;
                        }

                        if (createIncident)
                        {
                            AlertResult diskResult = new AlertResult() { ResourceName = HostName, InstanceName = match.Value };
                            diskResult.PartitionKey = diskResult.ResourceName + " - " + diskResult.InstanceName;
                            diskResult.Type = type;
                            if (!String.IsNullOrEmpty(Description))
                                diskResult.Description = Description;
                            else
                                diskResult.Description = $"Disk alert for drive: {diskResult.InstanceName} - size: {diskSize} - free percent: {freeSpacePercent} - free mb: {freeMegabytes}";

                            result.Results.Add(diskResult);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error in CustomLogServiceDISK");
            }

            return result;
        }

        protected int GetColumnIndex(string columnName, Newtonsoft.Json.Linq.JObject tableObject)
        {
            int ResourceIndex = -1;

            for (int p = 0; p < ((JArray)tableObject["columns"]).Count; p++)
            {
                if (tableObject["columns"][p]["name"].ToString() == columnName)
                {
                    ResourceIndex = p;
                    break;
                }
            }

            return ResourceIndex;
        }
    }
}
