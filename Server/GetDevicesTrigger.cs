using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Devices;
using System.Collections.Generic;
using Microsoft.Azure.Devices.Shared;

namespace Server.Triggers;

public class GetDevicesTrigger
{
    [FunctionName("GetDevicesTrigger")]
    public async Task<IActionResult> GetDevices(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "devices")] HttpRequest req,
        ILogger log)
    {
        RegistryManager registryManager = RegistryManager.CreateFromConnectionString(Environment.GetEnvironmentVariable("IotHubAdminConnectionString"));
        var devices = registryManager.CreateQuery("SELECT * FROM devices");
        List<Twin> twins = new List<Twin>();
        while (devices.HasMoreResults)
        {
            var page = await devices.GetNextAsTwinAsync();
            foreach (var twin in page)
            {
                twins.Add(twin);
            }
        }

        return new OkObjectResult(twins);
    }
}

