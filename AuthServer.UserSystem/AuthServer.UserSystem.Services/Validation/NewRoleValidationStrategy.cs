using AuthServer.UserSystem.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthServer.Common.Validation;

namespace AuthServer.UserSystem.Services.Validation
{
    public class NewRoleValidationStrategy : FluentValidatorBasedStrategy<RoleCreateModel>, IValidationStrategy<RoleCreateModel>
    {
        public void Validate(RoleCreateModel obj, IValidator validator, string prefix = "")
        {

        }

        protected override void InitRules(FluentValidator rules)
        { 

        }
    }
}
