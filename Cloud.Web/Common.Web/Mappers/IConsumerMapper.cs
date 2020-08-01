using Common.Web.UI.Models.ViewModels;
using System.Collections.Generic;

namespace Common.Web.Mappers
{
    public interface IConsumerMapper
    {
        IEnumerable<ConsumerViewModel> MapConsumers(IEnumerable<ConsumerMessage> consumers);
        ConsumerViewModel MapConsumer(ConsumerMessage consumer);
    }
}
