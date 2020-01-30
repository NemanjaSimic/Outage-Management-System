namespace OMS.Web.Services.Queries
{
    using MediatR;
    using OMS.Web.UI.Models.ViewModels;

    public class GetTopologyQuery : IRequest<OmsGraph>
    {
    }
}
