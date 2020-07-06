using OMS.Common.Cloud;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.OmsContracts.DataContracts.HistoryModel
{
    public class EquipmentHistorical
    {
        public long Id { get; set; }
        public long EquipmentId { get; set; }
        public long? OutageId { get; set; }
        public DateTime OperationTime { get; set; }
        public DatabaseOperation DatabaseOperation { get; set; }
    }
}
