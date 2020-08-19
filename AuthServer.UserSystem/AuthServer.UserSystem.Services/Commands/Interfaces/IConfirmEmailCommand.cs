using System.Threading.Tasks;

namespace AuthServer.UserSystem.Services.Commands.Interfaces
{
    public interface IConfirmEmailCommand
    {
        Task<bool> Execute(string code);
    }
}
