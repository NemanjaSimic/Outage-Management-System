using Common.PubSubContracts.DataContracts.OMS;
using Common.Web.Models.ViewModels;
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
