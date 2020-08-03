using Common.Web.Models.ViewModels;
using MediatR;
using System.Collections.Generic;

namespace Common.Web.Services.Queries
{
    public class GetArchivedOutagesQuery : IRequest<IEnumerable<ArchivedOutageViewModel>>
    {
    }
}
