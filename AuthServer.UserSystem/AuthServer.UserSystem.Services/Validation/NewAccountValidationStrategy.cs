using AuthServer.UserSystem.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthServer.Common.Validation;

namespace AuthServer.UserSystem.Services.Validation
{
    public class NewAccountValidationStrategy : FluentValidatorBasedStrategy<AccountModel>, IValidationStrategy<AccountModel>
    {
        public void Validate(AccountModel obj, IValidator validator, string prefix = "")
        {

        }

        protected override void InitRules(FluentValidator rules)
        {

        }
    }
}
