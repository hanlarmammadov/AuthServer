using AuthServer.UserSystem.Domain.Entities;
using AuthServer.UserSystem.Services.Models;
using FluentValidation;
using AuthServer.Common.Validation;

namespace AuthServer.UserSystem.Services.Validation
{
    public class NewContactValidationStrategy : FluentValidatorBasedStrategy<ContactModel>, IValidationStrategy<ContactModel>
    {

        public void Validate(ContactModel obj, AuthServer.Common.Validation.IValidator validator, string prefix = "")
        {
            var fvResult = _rules.Validate(obj);
            if (!fvResult.IsValid)
                foreach (var err in fvResult.Errors)
                    validator.AddError(err.ErrorMessage, prefix + err.PropertyName);
        }

        protected override void InitRules(FluentValidator rules)
        {
            rules.RuleFor(user => user.Value).NotNull()
                                             .WithMessage("Required")
                                             .WithName("FirstName")
                                             .When(u => u.Type == UserContactType.Email);

            rules.RuleFor(user => user.Value).NotNull()
                                             .WithMessage("Required")
                                             .WithName("FirstName")
                                             .When(u => u.Type == UserContactType.Mobile);
        }
    }
}
