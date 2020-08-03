using Common.Web.UI.Models.ViewModels;
using MediatR;

namespace Common.Web.Services.Queries
{
    public class GetTopologyQuery : IRequest<OmsGraphViewModel>
    {
    }
}
