namespace OMS.Web.Adapter.Contracts
{
    using Outage.Common.UI;
    
    public interface ITopologyClient
    {
        UIModel GetTopology();
    }
}
