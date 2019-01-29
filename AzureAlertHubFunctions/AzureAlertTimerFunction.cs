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
    public static class AzureAlertTimerFunction
    {
        static IAlertUtilities alertUtils;

        static AzureAlertTimerFunction()
        {
            string ServiceManagementHost = System.Environment.GetEnvironmentVariable("ServiceManagementType");
            if (ServiceManagementHost.ToUpper() == "SNOW")
            {
                alertUtils = new AlertUtilities(new ServiceManagementSNOW());
            }
            else
            {
                alertUtils = new AlertUtilities(new ServiceManagementTEST());
            }
        }

        [FunctionName("AzureAlertTimerFunction")]
        public static void Run([TimerTrigger("0 */60 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"AzureAlertConfirmTimerFunction Timer trigger function executed at: {DateTime.Now}");

            alertUtils.CheckAlertsStatus(log);

            log.LogInformation("Finished checking incident status");
        }
    }
}
