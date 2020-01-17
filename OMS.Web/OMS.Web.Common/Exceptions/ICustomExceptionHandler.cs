using System.Net;
using System.Web.Http.Filters;

namespace OMS.Web.Common.Exceptions
{
    public interface ICustomExceptionHandler
    {
        HttpStatusCode StatusCode { get; set; }

        void Handle(HttpActionExecutedContext context);
    }
}
