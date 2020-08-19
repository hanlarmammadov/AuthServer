using AuthServer.Common.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using AuthServer.Common.Localization;

namespace AuthServer.Common.Validation
{
    public interface IValidator
    {
        ICollection<Message> Errors { get; }
        bool HasErrors { get; }
        void AddError(string errorText, string field);
        void AddError(string errorText);
        void AddErrors(List<Message> errors);
        ICultureProvider CultureProvider { get; }
    }
}
