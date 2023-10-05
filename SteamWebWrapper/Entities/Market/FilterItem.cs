using System.Text.Json.Serialization;

namespace SteamWebWrapper.Entities.Market;

public class FilterItem
{
    [JsonPropertyName("color")]
    public string? Color { get; set; }
    
    [JsonPropertyName("localized_name")]
    public required string LocalizedName { get; set; }

    [JsonPropertyName("matches")]
    public required string Matches { get; set; }
}