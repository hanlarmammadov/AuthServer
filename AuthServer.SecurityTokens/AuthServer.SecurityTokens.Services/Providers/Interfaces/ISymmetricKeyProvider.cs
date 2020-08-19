using Microsoft.IdentityModel.Tokens;

namespace AuthServer.SecurityTokens.Services.StartupConfigs
{
    public interface ISymmetricKeyProvider
    {
        SymmetricSecurityKey GetKey();
    }
}
