namespace OMS.Web.Common.Mappers
{
    using OMS.Web.UI.Models.ViewModels;
    using Outage.Common.PubSub.OutageDataContract;
    using System.Collections.Generic;
    using System.Linq;

    public class OutageMapper : IOutageMapper
    {
        private IConsumerMapper _consumerMapper;

        public OutageMapper(IConsumerMapper consumerMapper)
        {
            _consumerMapper = consumerMapper;
        }

        public ActiveOutageViewModel MapActiveOutage(ActiveOutageMessage outage)
            => new ActiveOutageViewModel
            {
                Id = outage.OutageId,
                ReportedAt = outage.ReportTime,
                ElementId = outage.OutageElementGid,
                AffectedConsumers = _consumerMapper.MapConsumers(outage.AffectedConsumers),
                DefaultIsolationPoints = outage.DefaultIsolationPoints,
                OptimalIsolationPoints = outage.OptimumIsolationPoints,
            };

        public IEnumerable<ActiveOutageViewModel> MapActiveOutages(IEnumerable<ActiveOutageMessage> outages)
            => outages.Select(o => MapActiveOutage(o)).ToList();

        public ArchivedOutageViewModel MapArchivedOutage(ArchivedOutageMessage outage)
            => new ArchivedOutageViewModel
            {
                Id = outage.OutageId,
                ReportedAt = outage.ReportTime,
                ArchivedAt = outage.ArchiveTime,
                ElementId = outage.OutageElementGid,
                AffectedConsumers = _consumerMapper.MapConsumers(outage.AffectedConsumers),
                DefaultIsolationPoints = outage.DefaultIsolationPoints,
                OptimalIsolationPoints = outage.OptimumIsolationPoints,
            };

        public IEnumerable<ArchivedOutageViewModel> MapArchivedOutages(IEnumerable<ArchivedOutageMessage> outages)
            => outages.Select(o => MapArchivedOutage(o));
    }
}
