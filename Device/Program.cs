using System.Text;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Device.Models;
using Newtonsoft.Json;

var connectionString = "HostName=lab07-iothub.azure-devices.net;DeviceId=vendingmachine1;SharedAccessKey=4Y2VVlAl6cvRz3esPIOnwjSbShURpVQ/pAIoTMBSX0E=";

var deviceClient = DeviceClient.CreateFromConnectionString(connectionString);

float waterPrice = 1.5f;
float colaPrice = 2.5f;
float fruitJuicePrice = 2.5f;

int waterStock = new Random().Next(0, 10);
int colaStock = new Random().Next(0, 10);
int fruitJuiceStock = new Random().Next(0, 10);

SaleData currentTransaction = null;

await UpdateStockAsync();
await deviceClient.SetReceiveMessageHandlerAsync(ReceiveMessage, null);
await deviceClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertyChanged, null);

while (true)
{
    await Task.Delay(new Random().Next(5000, 20000));
    await NewOrderAsync();
}

async Task NewOrderAsync()
{
    var saleData = new SaleData
    {
        Drink = new Random().Next(0, 3) switch
        {
            0 => "water",
            1 => "cola",
            2 => "fruit juice",
            _ => throw new Exception("Invalid drink")
        },
        Quantity = new Random().Next(1, 4),
        LocationId = "GKG",
        DeviceId = "vendingmachine1"
    };

    if (!await InStockAsync(saleData.Drink, saleData.Quantity))
    {
        Console.WriteLine($"Out of stock: {saleData}");
        return;
    }

    currentTransaction = saleData;

    var message = new Message(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(saleData)));
    await deviceClient.SendEventAsync(message);

    Console.WriteLine($"New order: {saleData}");
}

async Task UpdateStockAsync()
{
    TwinCollection reportedProperties = new TwinCollection
    {
        ["waterStock"] = waterStock,
        ["colaStock"] = colaStock,
        ["fruitJuiceStock"] = fruitJuiceStock
    };

    await deviceClient.UpdateReportedPropertiesAsync(reportedProperties);
}

async Task ReceiveMessage(Message message, object userContext)
{
    var messageData = Encoding.ASCII.GetString(message.GetBytes());
    Console.WriteLine($"Received message: {messageData}");
    if (messageData == "Transaction completed")
    {
        switch (currentTransaction.Drink)
        {
            case "water":
                waterStock -= currentTransaction.Quantity;
                break;
            case "cola":
                colaStock -= currentTransaction.Quantity;
                break;
            case "fruit juice":
                fruitJuiceStock -= currentTransaction.Quantity;
                break;
        }

        currentTransaction = null;

        await UpdateStockAsync();
    }


    await deviceClient.CompleteAsync(message);
}

async Task OnDesiredPropertyChanged(TwinCollection desiredProperties, object userContext)
{
    waterPrice = desiredProperties["waterPrice"];
    colaPrice = desiredProperties["colaPrice"];
    fruitJuicePrice = desiredProperties["fruitJuicePrice"];
}

async Task<bool> InStockAsync(string drink, int quantity)
{
    switch (drink)
    {
        case "water":
            return waterStock >= quantity;
        case "cola":
            return colaStock >= quantity;
        case "fruit juice":
            return fruitJuiceStock >= quantity;
        default:
            throw new Exception("Invalid drink");
    }
}