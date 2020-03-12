using OMSCommon.OutageDatabaseModel;
using Outage.Common.PubSub.OutageDataContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMSCommon.Mappers
{
    public class EquipmentHistoricalMessageMapper
    {
        private OutageMessageMapper outageMapper;

        public EquipmentHistoricalMessageMapper(OutageMessageMapper outageMapper)
        {
            this.outageMapper = outageMapper;
        }

        public EquipmentHistoricalMessage MapEquipmentHistorical(EquipmentHistorical equipment)
        {
            EquipmentHistoricalMessage equipmentHistoricalMessage = new EquipmentHistoricalMessage()
            {
                EquipmentId = equipment.EquipmentId,
                OutageId = equipment.OutageId,
                OperationTime = equipment.OperationTime,
                DatabaseOperation = equipment.DatabaseOperation
            };

            return equipmentHistoricalMessage;
        }
    }
}
