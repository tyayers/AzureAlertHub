using AzureAlertHubFunctions.Dtos;
using AzureAlertHubFunctions.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace AzureAlertHubFunctions.Services
{
    public class SNOWManagement : IServiceManagement
    {
        public ServiceManagementResponseDto CreateIncident(AlertEntity alert)
        {
            ServiceManagementResponseDto response = null;

            using (HttpClient client = new HttpClient())
            {
                ServiceManagementDto payload = new ServiceManagementDto()
                {
                    caller_id = System.Environment.GetEnvironmentVariable("ServiceManagementCallerId"),
                    opened_by = System.Environment.GetEnvironmentVariable("ServiceManagementUser"),
                    business_service = System.Environment.GetEnvironmentVariable("ServiceManagementBusinessService"),
                    it_service = System.Environment.GetEnvironmentVariable("ServiceManagementITService"),
                    contact_type = System.Environment.GetEnvironmentVariable("ServiceManagementContactType"),
                    short_description = alert.AlertName,
                    description = alert.LogAnalyticsUrl,
                    assignment_group = System.Environment.GetEnvironmentVariable("ServiceManagementContactType"),
                    group_family = System.Environment.GetEnvironmentVariable("ServiceManagementGroupFamily"),
                    location = System.Environment.GetEnvironmentVariable("ServiceManagementLocation"),
                    gravity = Convert.ToInt16(System.Environment.GetEnvironmentVariable("ServiceManagementGravity")),
                    impact = Convert.ToInt16(System.Environment.GetEnvironmentVariable("ServiceManagementImpact"))
                };

                var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                HttpResponseMessage msg = client.PostAsync(System.Environment.GetEnvironmentVariable("ServiceManagementUrl"), content).Result;
                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = msg.Content.ReadAsStringAsync().Result;
                    response = Newtonsoft.Json.JsonConvert.DeserializeObject<ServiceManagementResponseDto>(JsonDataResponse);
                }
            }

            return response;
        }
    }
}
