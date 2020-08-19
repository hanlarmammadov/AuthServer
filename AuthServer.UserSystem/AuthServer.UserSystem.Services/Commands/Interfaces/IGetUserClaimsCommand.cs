using AuthServer.UserSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.UserSystem.Services.Commands.Interfaces
{
    public interface IGetUserClaimsCommand
    {
        Task<List<Claim>> Execute(UserClaimsRequest request);
    }
}
