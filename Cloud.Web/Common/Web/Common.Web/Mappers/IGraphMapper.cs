using Common.CeContracts;
using Common.PubSubContracts.DataContracts.CE.Interfaces;
using Common.Web.Models.ViewModels;

namespace Common.Web.Mappers
{
    public interface IGraphMapper
    {
        OmsGraphViewModel Map(IUIModel topologyModel);
    }
}
