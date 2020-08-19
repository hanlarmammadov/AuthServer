using System.Threading.Tasks;

namespace AuthServer.UserSystem.Services.Strategies.Interfaces
{
    public interface IEmailConfirmationStrategy
    {
        Task ImplementConfirmation(string accountId); 
    }
}
