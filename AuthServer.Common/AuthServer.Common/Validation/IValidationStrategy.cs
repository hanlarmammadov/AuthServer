using System;

namespace AuthServer.Common.Validation
{
    public interface IValidationStrategy<T>
    {
        void Validate(T obj, IValidator validator, string prefix = "");
    }
}
