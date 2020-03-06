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
        private IEquipmentMapper _equipmentMapper;

        public OutageMapper(IConsumerMapper consumerMapper, IEquipmentMapper equipmentMapper)
        {
            _consumerMapper = consumerMapper;
            _equipmentMapper = equipmentMapper;
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
                IsResolveConditionValidated = outage.IsResolveConditionValidated,
                DefaultIsolationPoints = outage.DefaultIsolationPoints.Select(o => o.EquipmentId),
                OptimalIsolationPoints = outage.OptimumIsolationPoints.Select(o => o.EquipmentId),
                //TODO: uncomment when using EquipmentViewModel
                //DefaultIsolationPoints = _equipmentMapper.MapEquipments(outage.DefaultIsolationPoints),
                //OptimalIsolationPoints = _equipmentMapper.MapEquipments(outage.OptimumIsolationPoints),
                AffectedConsumers = _consumerMapper.MapConsumers(outage.AffectedConsumers),
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
                ArchivedAt = outage.ArchivedTime,
                ElementId = outage.OutageElementGid,
                DefaultIsolationPoints = outage.DefaultIsolationPoints.Select(o => o.EquipmentId),
                OptimalIsolationPoints = outage.OptimumIsolationPoints.Select(o => o.EquipmentId),
                //TODO: uncomment when using EquipmentViewModel
                //DefaultIsolationPoints = _equipmentMapper.MapEquipments(outage.DefaultIsolationPoints),
                //OptimalIsolationPoints = _equipmentMapper.MapEquipments(outage.OptimumIsolationPoints),
                AffectedConsumers = _consumerMapper.MapConsumers(outage.AffectedConsumers),
            };

        public IEnumerable<ArchivedOutageViewModel> MapArchivedOutages(IEnumerable<ArchivedOutageMessage> outages)
            => outages.Select(o => MapArchivedOutage(o));

        public ActiveOutageLifecycleState MapActiveOutageState(OutageState activeOutageState)
        {
            switch(activeOutageState)
            {
                case OutageState.CREATED:
                    return ActiveOutageLifecycleState.Created;
                case OutageState.ISOLATED:
                    return ActiveOutageLifecycleState.Isolated;
                case OutageState.REPAIRED:
                    return ActiveOutageLifecycleState.Repaired;
                default:
                    throw new ArgumentException("Unsupported enum type. ActiveOutageState required.");
            }
        }
    }
}
