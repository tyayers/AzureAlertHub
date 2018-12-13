using AzureAlertHubFunctions.Dtos;
using AzureAlertHubFunctions.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace AzureAlertHubFunctions.Services
{
    public class SNOWManagement : IServiceManagement
    {
        public ServiceManagementResponseDto CreateIncident(AlertEntity alert, ILogger log)
        {
            ServiceManagementResponseDto response = null;

            using (HttpClient client = new HttpClient())
            {
                string serviceManagementCredentials = System.Environment.GetEnvironmentVariable("ServiceManagementCredentials");
                if (!String.IsNullOrEmpty(serviceManagementCredentials))
                {
                    var byteArray = Encoding.ASCII.GetBytes(serviceManagementCredentials);
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                }

                string serviceManagementUserAgent = System.Environment.GetEnvironmentVariable("ServiceManagementUserAgent");
                if (!String.IsNullOrEmpty(serviceManagementUserAgent)) 
                    client.DefaultRequestHeaders.Add("User-Agent", serviceManagementUserAgent);

                ServiceManagementDto payload = new ServiceManagementDto()
                {
                    caller_id = System.Environment.GetEnvironmentVariable("ServiceManagementCallerId"),
                    opened_by = System.Environment.GetEnvironmentVariable("ServiceManagementUser"),
                    business_service = System.Environment.GetEnvironmentVariable("ServiceManagementBusinessService"),
                    it_service = System.Environment.GetEnvironmentVariable("ServiceManagementITService"),
                    contact_type = System.Environment.GetEnvironmentVariable("ServiceManagementContactType"),
                    short_description = alert.AlertName,
                    description = alert.LogAnalyticsUrl,
                    assignment_group = System.Environment.GetEnvironmentVariable("ServiceManagementAssignmentGroup"),
                    group_family = System.Environment.GetEnvironmentVariable("ServiceManagementGroupFamily"),
                    location = System.Environment.GetEnvironmentVariable("ServiceManagementLocation"),
                    gravity = System.Environment.GetEnvironmentVariable("ServiceManagementGravity"),
                    impact = System.Environment.GetEnvironmentVariable("ServiceManagementImpact"),
                    stage = System.Environment.GetEnvironmentVariable("ServiceManagementStage")
                };

                var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                string snowUrl = System.Environment.GetEnvironmentVariable("ServiceManagementUrl");
                HttpResponseMessage msg = client.PostAsync(snowUrl, content).Result;
                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = msg.Content.ReadAsStringAsync().Result;
                    response = Newtonsoft.Json.JsonConvert.DeserializeObject<ServiceManagementResponseDto>(JsonDataResponse);
                }
                else
                {
                    var JsonDataResponse = msg.Content.ReadAsStringAsync().Result;
                    string responseMessage = "";
                    if (JsonDataResponse != null) responseMessage = JsonDataResponse.ToString();
                    log.LogError($"Could not reach SNOW server: {msg.ToString()} - {responseMessage}");
                }
            }

            return response;
        }
    }
}
