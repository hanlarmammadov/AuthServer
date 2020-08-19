using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthServer.UserSystem.Api
{
    public class IntToBoolModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            var modelName = bindingContext.ModelName;

            // Try to fetch the value of the argument by name
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
                return Task.CompletedTask;

            string value = valueProviderResult.FirstValue;
             
            // The string containing an integer value can be only "0" or "1" which correspond to false and true booleans accordingly.
            if (value == "false" || value == "0")
                bindingContext.Result = ModelBindingResult.Success(model: false);
            else if (value == "true" || value == "1")
                bindingContext.Result = ModelBindingResult.Success(model: true);

            return Task.CompletedTask; 
        }
    }
}
