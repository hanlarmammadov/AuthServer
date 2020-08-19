using AuthServer.Infrastructure.Jwt;
using AuthServer.SecurityTokens.Services.StartupConfigs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Threading.Tasks;

namespace AuthServer.SecurityTokens.Api.StartupConfigs
{
    public class JwtBearerConfigProvider
    {
        private readonly JwtConfig _jwtConfig;
        private readonly ISymmetricKeyProvider _symmetricKeyProvider;

        private Func<TokenValidatedContext, Task> GetOnTokenValidatedEventHandler()
        {
            Func<TokenValidatedContext, Task> handler = async context =>
            {
                //var prin = context.Principal;
                //var meaning = _dumbService.Meaning();
                //var claims = new List<Claim>
                //{
                //    new Claim(ClaimTypes.Role, "superadmin")
                //};


                //ClaimsIdentity identity = new ClaimsIdentity(claims, "AuthenticationTypes.Federation");
                //var newPrincipal = new ClaimsPrincipal(identity);
                //context.Principal = newPrincipal;
            };
            return handler;
        } 

        public JwtBearerConfigProvider(JwtConfig jwtConfig, ISymmetricKeyProvider symmetricKeyProvider)
        {
            _jwtConfig = jwtConfig;
            _symmetricKeyProvider = symmetricKeyProvider;
        }
        
        public Action<JwtBearerOptions> GetJwtBearerConfigurer()
        {
            Action<JwtBearerOptions> deleg = jwtBearerOptions =>
            {
                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateAudience = _jwtConfig.ValidateAudience,
                    ValidateIssuer = _jwtConfig.ValidateIssuer,
                    ValidateLifetime = _jwtConfig.ValidateLifetime,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _jwtConfig.ValidIssuer,
                    ValidAudience = _jwtConfig.ValidAudience,
                    IssuerSigningKey = _symmetricKeyProvider.GetKey(),
                    ClockSkew = TimeSpan.Zero
                };

                jwtBearerOptions.Events = new JwtBearerEvents()
                {
                    OnTokenValidated = GetOnTokenValidatedEventHandler()
                };
            };
            return deleg;
        }
    }
}
