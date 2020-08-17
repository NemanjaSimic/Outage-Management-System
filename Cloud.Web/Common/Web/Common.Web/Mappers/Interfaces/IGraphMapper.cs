using Common.PubSubContracts.DataContracts.CE.UIModels;
using Common.Web.Models.ViewModels;

namespace Common.Web.Mappers
{
    public interface IGraphMapper
    {
        OmsGraphViewModel Map(UIModel topologyModel);
    }
}
