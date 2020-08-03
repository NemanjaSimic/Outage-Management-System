using System.Net;
using System.Web.Http.Filters;

namespace Common.Web.Exceptions
{
    public interface ICustomExceptionHandler
    {
        HttpStatusCode StatusCode { get; set; }

        void Handle(HttpActionExecutedContext context);
    }
}
