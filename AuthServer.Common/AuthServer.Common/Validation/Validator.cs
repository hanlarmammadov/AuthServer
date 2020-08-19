using AuthServer.Common.Localization;
using AuthServer.Common.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuthServer.Common.Validation
{
    public class Validator : IValidator
    {
        private ICultureProvider _cultureProvider;
        private ICollection<Message> _errors;
        public ICollection<Message> Errors { get { return _errors; } }

        public Validator(ICultureProvider cultureProvider)
        {
            _cultureProvider = cultureProvider;
            _errors = new List<Message>();
        }

        public bool HasErrors
        {
            get
            {
                return (_errors.Count != 0);
            }
        }

        ICultureProvider IValidator.CultureProvider => _cultureProvider;

        public void AddError(string text, string field)
        {
            _errors.Add(new Message(_cultureProvider.Localize(text), field));
        }

        public void AddError(string errorMessage)
        {
            AddError(_cultureProvider.Localize(errorMessage), null);
        }

        public void AddErrors(List<Message> errors)
        {
            throw new NotImplementedException();
        }
    }
}
