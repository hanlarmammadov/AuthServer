using AuthServer.UserSystem.Models;
using AuthServer.Common.Validation;

namespace AuthServer.UserSystem.Services.Validation
{
    public class PasswordChangeValidationStrategy : FluentValidatorBasedStrategy<PasswordChangeModel>, IValidationStrategy<PasswordChangeModel>
    {
        public void Validate(PasswordChangeModel obj, IValidator validator, string prefix = "")
        {

        }

        protected override void InitRules(FluentValidator rules)
        {

        }
    }
}
