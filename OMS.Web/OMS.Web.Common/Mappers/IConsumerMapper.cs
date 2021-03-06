﻿namespace OMS.Web.Common.Mappers
{
    using OMS.Web.UI.Models.ViewModels;
    using Outage.Common.PubSub.OutageDataContract;
    using System.Collections.Generic;

    public interface IConsumerMapper
    {
        IEnumerable<ConsumerViewModel> MapConsumers(IEnumerable<ConsumerMessage> consumers);
        ConsumerViewModel MapConsumer(ConsumerMessage consumer);
    }
}
