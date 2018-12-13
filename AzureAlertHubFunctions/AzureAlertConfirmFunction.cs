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
            if (String.IsNullOrEmpty(ServiceManagementHost) || ServiceManagementHost.ToUpper() == "TEST")
            {
                alertUtils = new AlertUtilities(new TESTManagement());
            }
            else
            {
                alertUtils = new AlertUtilities(new SNOWManagement());
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

                AlertIncidentEntity alertIncident = alertUtils.RetrieveAlertEntity("SNOW", IncidentId);
                if (alertIncident != null)
                {
                    AlertEntity alert = alertUtils.RetrieveAlert(alertIncident.AlertPartitionId, alertIncident.AlertRowId);
                    alertUtils.DeleteAlert(alert);
                    alertUtils.DeleteAlertIncident(alertIncident);
                }
            }
            else
                log.LogError("Missing parameter incidentid to know which alert to close!");

            return (ActionResult)new OkObjectResult(result);
        }
    }
}
