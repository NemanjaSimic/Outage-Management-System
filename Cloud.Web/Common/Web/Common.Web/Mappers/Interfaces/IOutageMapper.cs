using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using Common.PubSubContracts.DataContracts.OMS;
using Common.Web.Models.ViewModels;
using System.Collections.Generic;

namespace Common.Web.Mappers
{
    public interface IOutageMapper
    {
        ActiveOutageViewModel MapActiveOutage(OutageEntity outage);
        ActiveOutageViewModel MapActiveOutage(ActiveOutageMessage outage);
        ArchivedOutageViewModel MapArchivedOutage(OutageEntity outage);
        ArchivedOutageViewModel MapArchivedOutage(ArchivedOutageMessage outage);
        IEnumerable<ActiveOutageViewModel> MapActiveOutages(IEnumerable<OutageEntity> outages);
        IEnumerable<ActiveOutageViewModel> MapActiveOutages(IEnumerable<ActiveOutageMessage> outages);
        IEnumerable<ArchivedOutageViewModel> MapArchivedOutages(IEnumerable<OutageEntity> outages);
        IEnumerable<ArchivedOutageViewModel> MapArchivedOutages(IEnumerable<ArchivedOutageMessage> outages);
    }
}
