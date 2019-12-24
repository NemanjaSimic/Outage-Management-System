using OMS.Web.Common.Interfaces.Exceptions;
using Outage.Common;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http.Filters;

namespace OMS.Web.API.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CustomExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger _logger;

        public CustomExceptionFilterAttribute(ILogger logger)
        {
            _logger = logger;
        }

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

            _logger.LogError(null, context.Exception);

            base.OnException(context);
        }
    }
}