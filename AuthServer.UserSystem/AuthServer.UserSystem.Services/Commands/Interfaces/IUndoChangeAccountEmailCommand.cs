using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.UserSystem.Services.Commands.Interfaces
{
    public interface IUndoChangeAccountEmailCommand
    {
        Task<bool> Execute(string emailChangeRecordId);
    }
}
