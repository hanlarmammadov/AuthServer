using AuthServer.UserSystem.Models;
using System.Threading.Tasks;

namespace AuthServer.UserSystem.Services.Commands.Interfaces
{
    public interface IValidateCredentialsCommand
    {
        Task<(bool Result, string AccountId)> Execute(AuthValidationRequest request);
    }
}
