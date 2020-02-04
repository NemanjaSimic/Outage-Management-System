using MediatR;
using Outage.Common.ServiceContracts.OMS;
using System.Collections.Generic;

namespace OMS.Web.Services.Queries
{
    public class GetActiveOutagesQuery : IRequest<IEnumerable<ActiveOutage>>
    {
    }
}
