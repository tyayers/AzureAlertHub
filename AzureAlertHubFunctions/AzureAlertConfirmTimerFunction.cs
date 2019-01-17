using System;
using System.Collections.Generic;
using AzureAlertHubFunctions.Dtos;
using AzureAlertHubFunctions.Interfaces;
using AzureAlertHubFunctions.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AzureAlertHubFunctions
{
    public static class AzureAlertConfirmTimerFunction
    {
        static IAlertUtilities alertUtils;

        static AzureAlertConfirmTimerFunction()
        {
            string ServiceManagementHost = System.Environment.GetEnvironmentVariable("ServiceManagementType");
            if (ServiceManagementHost.ToUpper() == "SNOW")
            {
                alertUtils = new AlertUtilities(new SNOWManagement());
            }
            else
            {
                alertUtils = new AlertUtilities(new TESTManagement());
            }
        }

        [FunctionName("AzureAlertConfirmTimerFunction")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"AzureAlertConfirmTimerFunction Timer trigger function executed at: {DateTime.Now}");

            alertUtils.CheckIncidentsStatus(log);

            log.LogInformation("Finished checking incident status");
        }
    }
}
