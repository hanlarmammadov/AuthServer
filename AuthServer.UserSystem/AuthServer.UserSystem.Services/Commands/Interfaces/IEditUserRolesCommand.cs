using AuthServer.Common.Patterns;
using AuthServer.UserSystem.Models.Events;
using AuthServer.UserSystem.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthServer.Common.Validation;

namespace AuthServer.UserSystem.Services.Commands.Interfaces
{
    public interface IEditUserRolesCommand
    {
        Task Execute(CreateUserModel userModel, IValidator validator);
    }
}
