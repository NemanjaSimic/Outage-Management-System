using Common.Web.Models.ViewModels;
using MediatR;
using Microsoft.AspNet.SignalR.Client.Http;

namespace Common.Web.Services.Queries
{
    public class GetTopologyQuery : IRequest<OmsGraphViewModel>
    {
    }
}
