using AuthServer.UserSystem.Services.Models;
using System.Threading.Tasks;
using AuthServer.Common.Messages;

namespace AuthServer.UserSystem.Services.Queries.Interfaces
{
    public interface IGetUsersQuery
    {
        Task<IPage<UserListModel>> Execute(UserQueryModel userQuery);
    }
}
