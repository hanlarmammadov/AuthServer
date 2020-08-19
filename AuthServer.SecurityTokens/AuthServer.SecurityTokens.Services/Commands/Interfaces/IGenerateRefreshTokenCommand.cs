using AuthServer.SecurityTokens.Models;
using System;
using System.Collections.Generic; 
using System.Security.Claims; 
using System.Threading.Tasks;

namespace AuthServer.SecurityTokens.Services.Commands.Interfaces
{
    public interface IGenerateRefreshTokenCommand
    {
        Task<TokenResult> Execute(string accountId, List<Claim> claims, TokenAdditionalData additionalData, DateTime? customExpireDate = null);
    }
}
