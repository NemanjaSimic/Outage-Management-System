namespace OMS.Web.Common.Mappers
{
    using OMS.Web.UI.Models.ViewModels;
    using Outage.Common.PubSub.OutageDataContract;
    using System.Collections.Generic;

    public interface IOutageMapper
    {
        IEnumerable<ActiveOutageViewModel> MapActiveOutages(IEnumerable<ActiveOutageMessage> outages);
        IEnumerable<ArchivedOutageViewModel> MapArchivedOutages(IEnumerable<ArchivedOutageMessage> outages);
        
        ActiveOutageViewModel MapActiveOutage(ActiveOutageMessage outage);
        ArchivedOutageViewModel MapArchivedOutage(ArchivedOutageMessage outage);
    }
}
