using AuthServer.UserSystem.Services.Models;
using System.Threading.Tasks;

namespace AuthServer.UserSystem.Services.Queries.Interfaces
{
    public interface IGetUserDetailsQuery
    {
        Task<UserDetailedModel> Execute(string accountId);
    }
}
