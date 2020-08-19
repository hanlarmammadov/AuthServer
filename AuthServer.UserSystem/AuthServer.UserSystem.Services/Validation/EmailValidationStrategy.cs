using AuthServer.UserSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthServer.Common.Validation;

namespace AuthServer.UserSystem.Services.Validation
{
    public class EmailValidationStrategy : IValidationStrategy<ChangeEmailModel>
    {
        public void Validate(ChangeEmailModel obj, IValidator validator, string prefix = "")
        {

        }
    }
}
