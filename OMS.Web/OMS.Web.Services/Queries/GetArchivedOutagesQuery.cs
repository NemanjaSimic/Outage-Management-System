namespace OMS.Web.Services.Queries
{
    using MediatR;
    using OMS.Web.UI.Models.ViewModels;
    using System.Collections.Generic;

    public class GetArchivedOutagesQuery : IRequest<IEnumerable<ArchivedOutageViewModel>>
    {
    }
}
