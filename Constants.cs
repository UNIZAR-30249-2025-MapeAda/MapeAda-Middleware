using System.IdentityModel.Tokens.Jwt;

namespace MapeAda_Middleware;

public static class Constants
{
    // HttpClientFactory
    public const string BackendHttpClientName = "BackendClient";
    
    // IOptions
    public const string BackendConfigurationKey = "BackendConfiguration";
    public const string AuthConfigurationKey = "AuthConfiguration";

    // Jwt
    public const string JwtEmailKey = JwtRegisteredClaimNames.Sub;
    public const string JwtNipKey = "nip";
    public const string JwtRolKey = "rol";
    
    // Auth policies
    public const string GerenteOnlyPolicyName = "GerenteOnly";
}