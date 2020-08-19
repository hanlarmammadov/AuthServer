using AuthServer.UserSystem.Services.Models;
using System.Threading.Tasks;
using AuthServer.Common.Validation;

namespace AuthServer.UserSystem.Services.Commands.Interfaces
{
    public interface ICreateAccountCommand
    {
        Task<string> Execute(AccountModel accountModel, IValidator validator);
    }
}
