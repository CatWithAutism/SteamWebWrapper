using System.Text.Json.Serialization;

namespace SteamMarketSDK.Core.Contracts.Entities.Authorization;

public class TransferParameters
{
	[JsonPropertyName("auth")] public required string Auth { get; set; }

	[JsonPropertyName("remember_login")] public required bool RememberLogin { get; set; }

	[JsonPropertyName("steamid")] public required string SteamId { get; set; }

	[JsonPropertyName("token_secure")] public required string TokenSecure { get; set; }
}