using System;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Web.Http.Filters;
using OMS.Web.Common.Interfaces.Exceptions;

namespace OMS.Web.Common
{
    public class CustomExceptionBase : Exception, ICustomExceptionHandler
    {
        public virtual HttpStatusCode StatusCode { get; set; } = HttpStatusCode.InternalServerError;

        public CustomExceptionBase() : base() { }

        public CustomExceptionBase(string message) : base(message) { }

        public CustomExceptionBase(string message, Exception innerException) : base(message, innerException) { }

        public void Handle(HttpActionExecutedContext context)
        {
            context.Response = new HttpResponseMessage()
            {
                Content = new StringContent(this.Message, Encoding.UTF8, "application/json"),
                StatusCode = this.StatusCode
            };
        }
    }
}
