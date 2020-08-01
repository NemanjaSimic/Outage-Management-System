using CECommon;
using Common.Web.UI.Models.ViewModels;

namespace Common.Web.Mappers
{
    public interface IGraphMapper
    {
        OmsGraphViewModel Map(UIModel topologyModel);
    }
}
