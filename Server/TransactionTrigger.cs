using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;
using Microsoft.Azure.EventHubs;
using System.Text;
using Device.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Devices;

namespace Server.Triggers;

public class TransactionTrigger
{
    [FunctionName("TransactionTrigger")]
    public async Task Transaction(
        [IoTHubTrigger("messages/events", Connection = "EventHubEndpoint")] EventData message,
        ILogger log)
    {
        var data = JsonConvert.DeserializeObject<SaleData>(Encoding.UTF8.GetString(message.Body.Array));
        if (data != null && !string.IsNullOrEmpty(data.LocationId))
        {
            try
            {
                var clientOptions = new CosmosClientOptions()
                {
                    ConnectionMode = ConnectionMode.Gateway
                };

                var client = new CosmosClient(Environment.GetEnvironmentVariable("CosmosDBConnectionString"), clientOptions);
                var container = client.GetContainer("cloud services", "SalesData");
                data.Id = Guid.NewGuid().ToString();
                await container.CreateItemAsync(data, new PartitionKey(data.LocationId));
                log.LogInformation($"C# IoT Hub trigger function processed a message: {Encoding.UTF8.GetString(message.Body.Array)}");


                ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(Environment.GetEnvironmentVariable("IoTHubServiceConnectionString"));
                var commandMessage = new Message(Encoding.ASCII.GetBytes("Transaction completed"));
                await serviceClient.SendAsync(data.DeviceId, commandMessage);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }
        }
    }
}
