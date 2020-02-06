namespace OMS.Web.Common.Mappers
{
    using OMS.Web.UI.Models.ViewModels;
    using Outage.Common.PubSub.OutageDataContract;
    using System.Collections.Generic;
    using System.Linq;

    public class ConsumerMapper : IConsumerMapper
    {
        private IOutageMapper _outageMapper;

        public ConsumerMapper(IOutageMapper outageMapper)
        {
            _outageMapper = outageMapper;
        }

        public ConsumerViewModel MapConsumer(Consumer consumer)
            => new ConsumerViewModel
            {
                Id = consumer.ConsumerId,
                Mrid = consumer.ConsumerMRID,
                FirstName = consumer.FirstName,
                LastName = consumer.LastName,
                ActiveOutages = _outageMapper.MapActiveOutages(consumer.ActiveOutages),
                ArchivedOutages = _outageMapper.MapArchivedOutages(consumer.ArchivedOutages)
            };

        public IEnumerable<ConsumerViewModel> MapConsumers(IEnumerable<Consumer> consumers)
            => consumers.Select(c => MapConsumer(c)).ToList();       
    }
}
