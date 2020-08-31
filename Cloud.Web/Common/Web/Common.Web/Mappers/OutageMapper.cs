using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using Common.PubSubContracts.DataContracts.OMS;
using Common.Web.Models;
using Common.Web.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Web.Mappers
{
    public class OutageMapper : IOutageMapper
    {
        private IConsumerMapper _consumerMapper;
        private IEquipmentMapper _equipmentMapper;

        public OutageMapper(IConsumerMapper consumerMapper, IEquipmentMapper equipmentMapper)
        {
            _consumerMapper = consumerMapper;
            _equipmentMapper = equipmentMapper;
        }

        public ActiveOutageViewModel MapActiveOutage(OutageEntity outage)
            => new ActiveOutageViewModel
            {
                Id = outage.OutageId,
                State = (OutageLifecycleState)outage.OutageState,
                ReportedAt = outage.ReportTime,
                IsolatedAt = outage.IsolatedTime,
                RepairedAt = outage.RepairedTime,
                ElementId = outage.OutageElementGid,
                IsResolveConditionValidated = outage.IsResolveConditionValidated,
                DefaultIsolationPoints = _equipmentMapper.MapEquipments(outage.DefaultIsolationPoints),
                OptimalIsolationPoints = _equipmentMapper.MapEquipments(outage.OptimumIsolationPoints),
                AffectedConsumers = _consumerMapper.MapConsumers(outage.AffectedConsumers),
            };

        public ActiveOutageViewModel MapActiveOutage(ActiveOutageMessage outage)
            => new ActiveOutageViewModel
            {
                Id = outage.OutageId,
                State = (OutageLifecycleState)outage.OutageState,
                ReportedAt = outage.ReportTime,
                IsolatedAt = outage.IsolatedTime,
                RepairedAt = outage.RepairedTime,
                ElementId = outage.OutageElementGid,
                IsResolveConditionValidated = outage.IsResolveConditionValidated,
                DefaultIsolationPoints = _equipmentMapper.MapEquipments(outage.DefaultIsolationPoints),
                OptimalIsolationPoints = _equipmentMapper.MapEquipments(outage.OptimumIsolationPoints),
                AffectedConsumers = _consumerMapper.MapConsumers(outage.AffectedConsumers),
            };

        public ArchivedOutageViewModel MapArchivedOutage(OutageEntity outage)
            => new ArchivedOutageViewModel
            {
                Id = outage.OutageId,
                ReportedAt = outage.ReportTime,
                IsolatedAt = outage.IsolatedTime,
                RepairedAt = outage.RepairedTime,
                ArchivedAt = (DateTime)outage.ArchivedTime,
                ElementId = outage.OutageElementGid,
                DefaultIsolationPoints = _equipmentMapper.MapEquipments(outage.DefaultIsolationPoints),
                OptimalIsolationPoints = _equipmentMapper.MapEquipments(outage.OptimumIsolationPoints),
                AffectedConsumers = _consumerMapper.MapConsumers(outage.AffectedConsumers),
            };

        public ArchivedOutageViewModel MapArchivedOutage(ArchivedOutageMessage outage)
            => new ArchivedOutageViewModel
            {
                Id = outage.OutageId,
                ReportedAt = outage.ReportTime,
                IsolatedAt = outage.IsolatedTime,
                RepairedAt = outage.RepairedTime,
                ArchivedAt = outage.ArchivedTime,
                ElementId = outage.OutageElementGid,
                DefaultIsolationPoints = _equipmentMapper.MapEquipments(outage.DefaultIsolationPoints),
                OptimalIsolationPoints = _equipmentMapper.MapEquipments(outage.OptimumIsolationPoints),
                AffectedConsumers = _consumerMapper.MapConsumers(outage.AffectedConsumers),
            };

        public IEnumerable<ActiveOutageViewModel> MapActiveOutages(IEnumerable<OutageEntity> outages)
           => outages.Select(o => MapActiveOutage(o));

        public IEnumerable<ActiveOutageViewModel> MapActiveOutages(IEnumerable<ActiveOutageMessage> outages)
            => outages.Select(o => MapActiveOutage(o));

        public IEnumerable<ArchivedOutageViewModel> MapArchivedOutages(IEnumerable<OutageEntity> outages)
            => outages.Select(o => MapArchivedOutage(o));

        public IEnumerable<ArchivedOutageViewModel> MapArchivedOutages(IEnumerable<ArchivedOutageMessage> outages)
            => outages.Select(o => MapArchivedOutage(o));
    }
}
