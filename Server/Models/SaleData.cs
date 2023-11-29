using Newtonsoft.Json;

namespace Device.Models;

public class SaleData
{
    [JsonProperty("drink")]
    public string Drink { get; set; }
    [JsonProperty("quantity")]
    public int Quantity { get; set; }
    [JsonProperty("locationId")]
    public string LocationId { get; set; }
    [JsonProperty("deviceId")]
    public string DeviceId { get; set; }
    [JsonProperty("id")]
    public string Id { get; set; }
}
