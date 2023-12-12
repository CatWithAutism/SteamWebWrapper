using System.Text.Json.Serialization;

namespace SteamWebWrapper.Contracts.Entities.Market.ItemOrdersActivity;

[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
public class ItemOrdersActivityResponse
{
    [JsonPropertyName("success")]
    public long Success { get; set; }

    [JsonPropertyName("activity")]
    public List<Activity> Activity { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
}