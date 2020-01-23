﻿using Outage.Common.PubSub;
using Outage.Common.PubSub.CalculationEngineDataContract;
using Outage.Common.PubSub.SCADADataContract;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Outage.Common.ServiceContracts.PubSub
{
    [ServiceContract(CallbackContract = typeof(ISubscriberCallback))]
    public interface ISubscriber
    {
        [OperationContract(IsOneWay = true)]
        void Subscribe(Topic topic);
    }

    public interface ISubscriberCallback
    {
        [OperationContract]
        string GetSubscriberName();

        [OperationContract(IsOneWay = true)]
        [ServiceKnownType(typeof(SCADAMessage))]
        [ServiceKnownType(typeof(SingleAnalogValueSCADAMessage))]
        [ServiceKnownType(typeof(MultipleAnalogValueSCADAMessage))]
        [ServiceKnownType(typeof(SingleDiscreteValueSCADAMessage))]
        [ServiceKnownType(typeof(MultipleDiscreteValueSCADAMessage))]
        [ServiceKnownType(typeof(CalculationEngineMessage))]
        [ServiceKnownType(typeof(CalcualtionEnginePublication))]
        [ServiceKnownType(typeof(TopologyForUIMessage))]
        void Notify(IPublishableMessage message);
    }
}