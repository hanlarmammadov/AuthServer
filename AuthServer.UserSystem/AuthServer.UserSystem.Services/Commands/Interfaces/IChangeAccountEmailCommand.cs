using AuthServer.UserSystem.Models; 
using System.Threading.Tasks; 
using AuthServer.Common.Validation;

namespace AuthServer.UserSystem.Services.Commands.Interfaces
{
    public interface IChangeAccountEmailCommand
    {
        Task Execute(string accountId, ChangeEmailModel model, IValidator validator);
    }
}
