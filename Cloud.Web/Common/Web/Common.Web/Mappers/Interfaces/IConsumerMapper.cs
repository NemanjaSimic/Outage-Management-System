using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using Common.PubSubContracts.DataContracts.OMS;
using Common.Web.Models.ViewModels;
using System.Collections.Generic;

namespace Common.Web.Mappers
{
    public interface IConsumerMapper
    {
        ConsumerViewModel MapConsumer(Consumer consumer);
        ConsumerViewModel MapConsumer(ConsumerMessage consumer);
        IEnumerable<ConsumerViewModel> MapConsumers(IEnumerable<Consumer> consumers);
        IEnumerable<ConsumerViewModel> MapConsumers(IEnumerable<ConsumerMessage> consumers);
    }
}
