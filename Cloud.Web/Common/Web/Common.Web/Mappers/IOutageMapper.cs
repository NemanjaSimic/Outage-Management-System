using Common.Web.UI.Models.ViewModels;
using Outage.Common.PubSub.OutageDataContract;
using System.Collections.Generic;

namespace Common.Web.Mappers
{
    public interface IOutageMapper
    {
        IEnumerable<ActiveOutageViewModel> MapActiveOutages(IEnumerable<ActiveOutageMessage> outages);
        IEnumerable<ArchivedOutageViewModel> MapArchivedOutages(IEnumerable<ArchivedOutageMessage> outages);
        
        ActiveOutageViewModel MapActiveOutage(ActiveOutageMessage outage);
        ArchivedOutageViewModel MapArchivedOutage(ArchivedOutageMessage outage);
    }
}
