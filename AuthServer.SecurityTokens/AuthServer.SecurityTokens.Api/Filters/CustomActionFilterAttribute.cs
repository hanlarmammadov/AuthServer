using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthServer.SecurityTokens.Api.Filters
{ 
    public class CustomActionFilterAttribute : ActionFilterAttribute
    { 
        public override void OnActionExecuting(ActionExecutingContext context)
        { 

        }
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            //context.HttpContext.Response.Body.Write(bytes, 0, bytes.Length);
        }
    }
}
