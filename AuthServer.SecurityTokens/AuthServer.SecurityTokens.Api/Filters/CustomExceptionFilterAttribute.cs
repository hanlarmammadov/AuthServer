using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthServer.SecurityTokens.Api.Filters
{
    public class CustomExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is ArgumentOutOfRangeException)
            {
                 
                context.Result = new ViewResult()
                {
                    ViewName = "Message",
                    ViewData = new ViewDataDictionary( new EmptyModelMetadataProvider(), new ModelStateDictionary())
                    {
                        Model = @"The data received by the application cannot be processed"
                    }
                };
            }
        }
    }
}
