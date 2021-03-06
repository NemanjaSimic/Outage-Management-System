﻿using OMS.Web.Adapter.HubDispatchers;
using OMS.Web.Common.Mappers;
using Outage.Common.PubSub;
using Outage.Common.PubSub.OutageDataContract;
using Outage.Common.ServiceContracts.PubSub;
using System;
using System.Runtime.Serialization;
using System.ServiceModel;


namespace OMS.Web.Adapter.Outage
{
    [DataContract]
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class OutageNotification : ISubscriberCallback
    {
        private readonly string _subscriberName;
        private readonly OutageHubDispatcher _dispatcher;
        private readonly IOutageMapper _mapper;

        public OutageNotification(string subscriberName, IOutageMapper mapper)
        {
            _subscriberName = subscriberName;

            _mapper = mapper;
            _dispatcher = new OutageHubDispatcher(_mapper);
        }

        public string GetSubscriberName() => _subscriberName;

        public void Notify(IPublishableMessage message)
        {
            if (message is ActiveOutageMessage activeOutage)
            {
                _dispatcher.Connect();

                try
                {
                    _dispatcher.NotifyActiveOutageUpdate(activeOutage);
                }
                catch (Exception)
                {
                    //log error
                    // retry ?
                }
            }
            else if(message is ArchivedOutageMessage archivedOutage)
            {
                _dispatcher.Connect();

                try
                {
                    _dispatcher.NotifyArchiveOutageUpdate(archivedOutage);
                }
                catch (Exception)
                {
                    //log error
                    // retry ?
                }
            }
            else
            {
                //todo: if anything?
            }
        }
    }
}
