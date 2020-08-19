using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthServer.Common.Localization;

namespace AuthServer.Common.Validation
{
    public class ValidatorFactory : IValidatorFactory
    {
        private readonly ICultureProvider _cultureProvider;

        public ValidatorFactory(ICultureProvider cultureProvider)
        {
            this._cultureProvider = cultureProvider;
        }

        public IValidator Create()
        {
            Validator validator = new Validator(_cultureProvider);
            return validator;
        }
    }
}
