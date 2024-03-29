using System.Text.Json.Serialization;
using SteamWebWrapper.Contracts.Converters;

namespace SteamWebWrapper.Contracts.Entities.Market.MyHistory;

[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
public class MyHistoryEvent
{
    [JsonPropertyName("listingid")]
    public long ListingId { get; set; }

    [JsonPropertyName("event_type")]
    public MyHistoryEventType EventType { get; set; }

    [JsonPropertyName("time_event")]
    [JsonConverter(typeof(TimestampToDateConverter))]
    public DateTime TimeEvent { get; set; }

    [JsonPropertyName("time_event_fraction")]
    public long TimeEventFraction { get; set; }

    [JsonPropertyName("steamid_actor")]
    public long SteamIdActor { get; set; }

    [JsonPropertyName("date_event")]
    public string DateEvent { get; set; }
    
    [JsonPropertyName("purchaseid")]
    public long PurchaseId { get; set; }
}