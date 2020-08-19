using AuthServer.UserSystem.Domain.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.UserSystem.Data.Queries.Interfaces
{
    public interface IGetPasswordResetRequestByEmailAndCodeQuery : ICustomQuery<PasswordResetRequest, IMongoCollection<PasswordResetRequest>>
    {
        IGetPasswordResetRequestByEmailAndCodeQuery SetEmail(string email);
        IGetPasswordResetRequestByEmailAndCodeQuery SetResetCode(string code);
    }
}
