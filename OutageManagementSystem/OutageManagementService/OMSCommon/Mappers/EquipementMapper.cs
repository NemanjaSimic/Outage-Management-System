using OMSCommon.OutageDatabaseModel;
using Outage.Common.PubSub.OutageDataContract;
using System.Collections.Generic;
using System.Linq;

namespace OMSCommon.Mappers
{
    public class EquipmentMapper
    {
        private OutageMessageMapper outageMapper;

        public EquipmentMapper(OutageMessageMapper outageMapper)
        {
            this.outageMapper = outageMapper;
        }

        public EquipmentMessage MapEquipment(Equipment equipment)
        {
            EquipmentMessage equipmentMessage = new EquipmentMessage()
            {
                EquipmentId = equipment.EquipmentId,
                EquipmentMRID = equipment.EquipmentMRID,
                ActiveOutages = new List<ActiveOutageMessage>(), //TODO: outageMapper.MapActiveOutages(consumer.ActiveOutages),
                ArchivedOutages = new List<ArchivedOutageMessage>(), //TODO: outageMapper.MapArchivedOutages(consumer.ArchivedOutages)
            };

            return equipmentMessage;
        }

        public IEnumerable<EquipmentMessage> MapEquipments(IEnumerable<Equipment> consumers)
            => consumers.Select(c => MapEquipment(c)).ToList();
    }
}
