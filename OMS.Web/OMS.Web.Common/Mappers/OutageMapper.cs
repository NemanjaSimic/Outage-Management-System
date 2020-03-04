namespace OMS.Web.Common.Mappers
{
    using OMS.Web.UI.Models;
    using OMS.Web.UI.Models.ViewModels;
    using Outage.Common;
    using Outage.Common.PubSub.OutageDataContract;
    using System;
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
                State = MapActiveOutageState(outage.OutageState),
                ReportedAt = outage.ReportTime,
                IsolatedAt = outage.IsolatedTime,
                RepairedAt = outage.RepairedTime,
                ElementId = outage.OutageElementGid,
                DefaultIsolationPoints = outage.DefaultIsolationPoints.Select(e => e.EquipmentId),
                AffectedConsumers = _consumerMapper.MapConsumers(outage.AffectedConsumers),
                OptimalIsolationPoints = outage.OptimumIsolationPoints.Select(e => e.EquipmentId),
                IsResolveConditionValidated = outage.IsResolveConditionValidated,
            };

        public IEnumerable<ActiveOutageViewModel> MapActiveOutages(IEnumerable<ActiveOutageMessage> outages)
            => outages.Select(o => MapActiveOutage(o)).ToList();

        public ArchivedOutageViewModel MapArchivedOutage(ArchivedOutageMessage outage)
            => new ArchivedOutageViewModel
            {
                Id = outage.OutageId,
                ReportedAt = outage.ReportTime,
                IsolatedAt = outage.IsolatedTime,
                RepairedAt = outage.RepairedTime,
                ArchivedAt = outage.ArchiveTime,
                ElementId = outage.OutageElementGid,
                DefaultIsolationPoints = outage.DefaultIsolationPoints.Select(e => e.EquipmentId),
                AffectedConsumers = _consumerMapper.MapConsumers(outage.AffectedConsumers),
                OptimalIsolationPoints = outage.OptimumIsolationPoints.Select(e => e.EquipmentId),
            };

        public IEnumerable<ArchivedOutageViewModel> MapArchivedOutages(IEnumerable<ArchivedOutageMessage> outages)
            => outages.Select(o => MapArchivedOutage(o));

        public ActiveOutageLifecycleState MapActiveOutageState(ActiveOutageState activeOutageState)
        {
            switch(activeOutageState)
            {
                case ActiveOutageState.CREATED:
                    return ActiveOutageLifecycleState.Created;
                case ActiveOutageState.ISOLATED:
                    return ActiveOutageLifecycleState.Isolated;
                case ActiveOutageState.REPAIRED:
                    return ActiveOutageLifecycleState.Repaired;
                default:
                    throw new ArgumentException("Unsupported enum type. ActiveOutageState required.");
            }
        }
    }
}
