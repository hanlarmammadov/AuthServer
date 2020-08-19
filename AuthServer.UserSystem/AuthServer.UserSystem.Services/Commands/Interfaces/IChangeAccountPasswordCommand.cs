using AuthServer.UserSystem.Models;
using System.Threading.Tasks;
using AuthServer.Common.Validation;

namespace AuthServer.UserSystem.Services.Commands.Interfaces
{
    public interface IChangeAccountPasswordCommand
    {
        Task Execute(string accountId, PasswordChangeModel model, IValidator validator);
    }
}
