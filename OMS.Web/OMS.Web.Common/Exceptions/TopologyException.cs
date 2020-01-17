using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http.Filters;

namespace OMS.Web.Common.Exceptions
{
    public class TopologyException : Exception, ICustomExceptionHandler
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.InternalServerError;

        public TopologyException() : base() { }

        public TopologyException(string message) : base(message) { }

        public TopologyException(string message, Exception innerException) : base(message, innerException) { }

        public void Handle(HttpActionExecutedContext context)
        {
            context.Response = new HttpResponseMessage()
            {
                Content = new StringContent(Message, Encoding.UTF8, "application/json"),
                StatusCode = HttpStatusCode.InternalServerError
            };
        }
    }
}
