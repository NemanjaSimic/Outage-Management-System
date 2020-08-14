using Common.CeContracts;
using Common.Web.Models.ViewModels;

namespace Common.Web.Mappers
{
    public interface IGraphMapper
    {
        OmsGraphViewModel Map(UIModel topologyModel);
    }
}
