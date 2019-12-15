using System;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Web.Http.Filters;
using OMS.Web.Common.Interfaces.Exceptions;

namespace OMS.Web.API.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CustomExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            var exceptionType = context.Exception.GetType();

            if (exceptionType is ICustomExceptionHandler customExceptionHandler)
            {
                customExceptionHandler.Handle(context);
            }
            else
            {
                context.Response = new HttpResponseMessage()
                {
                    Content = new StringContent("Internal Server Error", Encoding.UTF8, "text/plain"),
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
            base.OnException(context);
        }
    }
}