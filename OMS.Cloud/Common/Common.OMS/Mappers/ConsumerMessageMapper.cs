using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using Common.PubSubContracts.DataContracts.OMS;
using System.Collections.Generic;
using System.Linq;

namespace Common.OMS.Mappers
{
    public class ConsumerMessageMapper
    {
        private OutageMessageMapper outageMapper;

        public ConsumerMessageMapper(OutageMessageMapper outageMapper)
        {
            this.outageMapper = outageMapper;
        }

        public ConsumerMessage MapConsumer(Consumer consumer)
        {
            ConsumerMessage consumerMessage = new ConsumerMessage()
            {
                ConsumerId = consumer.ConsumerId,
                ConsumerMRID = consumer.ConsumerMRID,
                FirstName = consumer.FirstName,
                LastName = consumer.LastName,
                ActiveOutages = new List<ActiveOutageMessage>(), //MODO: outageMapper.MapActiveOutages(consumer.ActiveOutages),
                ArchivedOutages = new List<ArchivedOutageMessage>(), //MODO: outageMapper.MapArchivedOutages(consumer.ArchivedOutages)
            };

            return consumerMessage;
        }

        public IEnumerable<ConsumerMessage> MapConsumers(IEnumerable<Consumer> consumers)
            => consumers.Select(c => MapConsumer(c)).ToList();
    }
}

