using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using AzureAlertHubFunctions.Dtos;
using AzureAlertHubFunctions.Interfaces;
using AzureAlertHubFunctions.Services;

namespace AzureAlertHubFunctions
{
    public static class AzureAlertConfirmFunction
    {
        static IAlertUtilities alertUtils;
        
        static AzureAlertConfirmFunction()
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

        [FunctionName("AzureAlertConfirmFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string result = "Alert successfully confirmed.";

            // parse query parameter
            string IncidentId = "";
            if (req.GetQueryParameterDictionary().ContainsKey("incidentid"))
            {
                IncidentId = req.GetQueryParameterDictionary()["incidentid"];

                log.LogInformation($"Preparing to confirm SNOW incident {IncidentId}");

                AlertIncidentEntity alertIncident = alertUtils.GetAlertIncident("SNOW", IncidentId);
                if (alertIncident != null)
                {
                    AlertEntity alert = alertUtils.GetAlert(alertIncident.AlertPartitionId, alertIncident.AlertRowId);
                    alertUtils.DeleteAlert(alert);
                    alertUtils.DeleteAlertIncident(alertIncident);
                }
            }
            else
            {
                log.LogInformation("NO incidentid was passed, so checking all open incidents..");
                alertUtils.CheckIncidentsStatus(log);
            }

            log.LogInformation("Finished checking incident status");

            return (ActionResult)new OkObjectResult(result);
        }
    }
}
