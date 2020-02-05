using MediatR;
using OMS.Web.UI.Models.ViewModels;
using System.Collections.Generic;

namespace OMS.Web.Services.Queries
{
    public class GetArchivedOutagesQuery : IRequest<IEnumerable<ArchivedOutage>>
    {
    }
}
