namespace OMS.Web.Common.Mappers
{
    using OMS.Web.UI.Models.ViewModels;
    using Outage.Common.PubSub.OutageDataContract;
    using System.Collections.Generic;

    public interface IOutageMapper
    {
        IEnumerable<ActiveOutageViewModel> MapActiveOutages(IEnumerable<ActiveOutage> outages);
        IEnumerable<ArchivedOutageViewModel> MapArchivedOutages(IEnumerable<ArchivedOutage> outages);
        
        ActiveOutageViewModel MapActiveOutage(ActiveOutage outage);
        ArchivedOutageViewModel MapArchivedOutage(ArchivedOutage outage);
    }
}
