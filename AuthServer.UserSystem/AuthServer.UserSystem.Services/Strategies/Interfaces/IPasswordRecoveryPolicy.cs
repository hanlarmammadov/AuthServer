using AuthServer.UserSystem.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthServer.Common.Validation;

namespace AuthServer.UserSystem.Services.Strategies.Interfaces
{ 
    public interface IPasswordRecoveryPolicy
    {
        void RequestPasswordRecovery(PasswordRecoveryRequestModel model, IValidator validator);
        void CompletePasswordRecovery(CompletePasswordRecoveryModel model, IValidator validator);
    }
}
