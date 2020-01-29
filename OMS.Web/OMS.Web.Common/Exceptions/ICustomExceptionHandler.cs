namespace OMS.Web.Common.Exceptions
{
    using System.Net;
    using System.Web.Http.Filters;

    public interface ICustomExceptionHandler
    {
        HttpStatusCode StatusCode { get; set; }

        void Handle(HttpActionExecutedContext context);
    }
}
