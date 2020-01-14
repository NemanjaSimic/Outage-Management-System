using Outage.Common.UI;
using OMS.Web.UI.Models.ViewModels;

namespace OMS.Web.Common.Mappers
{
    public interface IGraphMapper
    {
        OmsGraph MapTopology(UIModel topologyModel);
    }
}
