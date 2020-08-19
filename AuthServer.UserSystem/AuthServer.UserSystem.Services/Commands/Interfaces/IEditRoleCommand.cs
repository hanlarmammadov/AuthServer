using AuthServer.UserSystem.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthServer.Common.Validation;

namespace AuthServer.UserSystem.Services.Commands.Interfaces
{
    public interface IEditRoleCommand
    { 
        Task Execute(string roleId, RoleCreateModel changes, IValidator validator);
    }
}
