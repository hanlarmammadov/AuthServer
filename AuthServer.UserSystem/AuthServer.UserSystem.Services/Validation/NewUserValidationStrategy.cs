using AuthServer.UserSystem.Domain.Entities;
using AuthServer.UserSystem.Services.Models;
using FluentValidation; 
using AuthServer.Common.Validation;

namespace AuthServer.UserSystem.Services.Validation
{  
    public class NewUserValidationStrategy : FluentValidatorBasedStrategy<CreateUserModel>, IValidationStrategy<CreateUserModel>
    {
        private readonly IValidationStrategy<ContactModel> _newContactValidationStrategy;

        public NewUserValidationStrategy(IValidationStrategy<ContactModel> newContactValidationStrategy)
        {
            _newContactValidationStrategy = newContactValidationStrategy;
        }

        public void Validate(CreateUserModel userModel, AuthServer.Common.Validation.IValidator validator, string prefix = "")
        {
            var fvResult = _rules.Validate(userModel);
            if (!fvResult.IsValid)
                foreach (var err in fvResult.Errors)
                    validator.AddError(err.ErrorMessage, prefix + err.PropertyName);

            if (userModel.Contacts != null) 
                for (int i = 0; i < userModel.Contacts.Count;)
                    _newContactValidationStrategy.Validate(userModel.Contacts[i], validator, $"{prefix}.{i++}"); 
        }

        protected override void InitRules(FluentValidator rules)
        {
            rules.RuleFor(user => user.FirstName).NotNull().WithMessage("Required").WithName("FirstName")
                                                 .MinimumLength(2).WithMessage("Minimum length is 2").WithName("FirstName")
                                                 .MaximumLength(20).WithMessage("Maximum length is 20").WithName("FirstName");

            rules.RuleFor(user => user.LastName).NotNull().WithMessage("Required").WithName("LastName")
                                                .MinimumLength(2).WithMessage("Minimum length is 2").WithName("LastName")
                                                .MaximumLength(20).WithMessage("Maximum length is 20").WithName("LastName");
             
            rules.RuleFor(user => user.Gender).NotEqual(Gender.NotSet).WithMessage("Required").WithName("LastName");
        }

        
    }
}
