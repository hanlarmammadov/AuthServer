using AuthServer.SecurityTokens.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.SecurityTokens.Services.Queries.Interfaces
{
    public interface IGetAllTokensForAccountQuery
    {
        Task<IEnumerable<AccountTokenModel>> Execute(string accountId);
    }
}
