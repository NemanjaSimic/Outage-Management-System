using Common.Web.UI.Models;
using Common.Web.UI.Models.ViewModels;
using Outage.Common.PubSub.OutageDataContract;
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
                DefaultIsolationPoints = _equipmentMapper.MapEquipments(outage.DefaultIsolationPoints),
                OptimalIsolationPoints = _equipmentMapper.MapEquipments(outage.OptimumIsolationPoints),
                AffectedConsumers = _consumerMapper.MapConsumers(outage.AffectedConsumers),
            };

        public IEnumerable<ArchivedOutageViewModel> MapArchivedOutages(IEnumerable<ArchivedOutageMessage> outages)
            => outages.Select(o => MapArchivedOutage(o));
    }
}
