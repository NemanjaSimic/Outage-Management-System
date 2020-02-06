namespace OMS.Web.Common.Mappers
{
    using OMS.Web.UI.Models.ViewModels;
    using Outage.Common.PubSub.OutageDataContract;
    using System.Collections.Generic;
    using System.Linq;

    public class ConsumerMapper : IConsumerMapper
    {
        public ConsumerViewModel MapConsumer(Consumer consumer)
            => new ConsumerViewModel
            {
                Id = consumer.ConsumerId,
                Mrid = consumer.ConsumerMRID,
                FirstName = consumer.FirstName,
                LastName = consumer.LastName,
                //ArchivedOutages = consumer.ArchivedOutages // kada uradite mapper za ovo
            };

        public IEnumerable<ConsumerViewModel> MapConsumers(IEnumerable<Consumer> consumers)
            => consumers.Select(c => MapConsumer(c)).ToList();       
    }
}
