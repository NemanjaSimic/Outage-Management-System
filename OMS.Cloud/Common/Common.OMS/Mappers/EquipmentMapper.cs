using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using Common.PubSubContracts.DataContracts.OMS;
using System.Collections.Generic;
using System.Linq;

namespace Common.OMS.Mappers
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
                ActiveOutages = new List<ActiveOutageMessage>(), //MODO: outageMapper.MapActiveOutages(consumer.ActiveOutages),
                ArchivedOutages = new List<ArchivedOutageMessage>(), //MODO: outageMapper.MapArchivedOutages(consumer.ArchivedOutages)
            };

            return equipmentMessage;
        }

        public IEnumerable<EquipmentMessage> MapEquipments(IEnumerable<Equipment> consumers)
            => consumers.Select(c => MapEquipment(c)).ToList();
    }
}

