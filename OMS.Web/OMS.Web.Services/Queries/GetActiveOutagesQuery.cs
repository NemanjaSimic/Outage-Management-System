using MediatR;
using Outage.Common.PubSub.OutageDataContract;
using System.Collections.Generic;

namespace OMS.Web.Services.Queries
{
    public class GetActiveOutagesQuery : IRequest<IEnumerable<ActiveOutage>>
    {
    }
}
