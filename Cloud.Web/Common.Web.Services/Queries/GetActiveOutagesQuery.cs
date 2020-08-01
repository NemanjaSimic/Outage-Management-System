using MediatR;
using System.Collections.Generic;

namespace Common.Web.Services.Queries
{
    public class GetActiveOutagesQuery : IRequest<IEnumerable<ActiveOutageViewModel>>
    {}
}
