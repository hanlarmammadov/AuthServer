using AuthServer.SecurityTokens.Models;
using System.Threading.Tasks;

namespace AuthServer.SecurityTokens.Services.Commands.Interfaces
{
    public interface IGenerateShortTokenCommand
    {
        Task<TokenResult> Execute(string refreshToken);
    }
}
