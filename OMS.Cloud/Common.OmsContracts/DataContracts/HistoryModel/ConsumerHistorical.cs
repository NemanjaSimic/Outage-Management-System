using OMS.Common.Cloud;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.OmsContracts.DataContracts.HistoryModel
{
    public class ConsumerHistorical
    {
        public long Id { get; set; }
        public long ConsumerId { get; set; }
        public long? OutageId { get; set; }
        public DateTime OperationTime { get; set; }
        public DatabaseOperation DatabaseOperation { get; set; }
    }
}
