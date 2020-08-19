using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.UserSystem.Services.Validation
{
    public abstract class FluentValidatorBasedStrategy<T>
    {
        protected FluentValidator _rules;

        protected FluentValidatorBasedStrategy()
        {
            _rules = new FluentValidator();
            InitRules(_rules);
        }

        protected abstract void InitRules(FluentValidator rules);

        protected class FluentValidator : AbstractValidator<T> { }
    }
}
