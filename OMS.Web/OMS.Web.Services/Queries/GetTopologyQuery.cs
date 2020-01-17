using MediatR;
using OMS.Web.UI.Models.ViewModels;

namespace OMS.Web.Services.Queries
{
    public class GetTopologyQuery : IRequest<OmsGraph>
    {
    }
}
