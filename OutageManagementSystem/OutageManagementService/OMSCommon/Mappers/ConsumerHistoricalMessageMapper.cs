using OMSCommon.OutageDatabaseModel;
using Outage.Common.PubSub.OutageDataContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMSCommon.Mappers
{
    public class ConsumerHistoricalMessageMapper
    {
        private OutageMessageMapper outageMapper;

        public ConsumerHistoricalMessageMapper(OutageMessageMapper outageMapper)
        {
            this.outageMapper = outageMapper;
        }

        public ConsumerHistoricalMessage MapConsumer(ConsumerHistorical consumer)
        {
            ConsumerHistoricalMessage consumerHistoricalMessage = new ConsumerHistoricalMessage()
            {
                ConsumerId = consumer.ConsumerId,
                OutageId = consumer.OutageId,
                OperationTime = consumer.OperationTime,
                DatabaseOperation = consumer.DatabaseOperation
            };

            return consumerHistoricalMessage;
        }

    }
}
