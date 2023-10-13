using System.Security;

namespace SteamWebWrapper.Core.Entities.Authorization;

[SecuritySafeCritical]
public class SteamAuthCredentials
{
    public required string Login { get; set; }
    
    public required string Password { get; set; }

    public required string TwoFactor { get; set; }

    public required string MachineName { get; set; }
}