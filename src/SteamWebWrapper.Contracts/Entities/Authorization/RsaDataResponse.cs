using System.Text.Json.Serialization;

namespace SteamWebWrapper.Contracts.Entities.Authorization;

public class RsaDataResponse
{
    [JsonPropertyName("success")]
    public required bool Success { get; set; }

    [JsonPropertyName("publickey_mod")]
    public string? PublicKeyMod { get; set; }

    [JsonPropertyName("publickey_exp")]
    public string? PublicKeyExp { get; set; }

    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }
}