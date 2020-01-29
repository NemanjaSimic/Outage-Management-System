namespace OMS.Web.Common.Mappers
{
    using Outage.Common.UI;
    using OMS.Web.UI.Models.ViewModels;

    public interface IGraphMapper
    {
        OmsGraph MapTopology(UIModel topologyModel);
    }
}
