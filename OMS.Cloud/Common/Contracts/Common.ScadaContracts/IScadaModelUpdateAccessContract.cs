using Microsoft.ServiceFabric.Services.Remoting;
using Outage.Common.PubSub.SCADADataContract;
using System.Collections.Generic;
using System.ServiceModel;

namespace OMS.Common.ScadaContracts
{
    [ServiceContract]
    public interface IScadaModelUpdateAccessContract : IService
    {
        [OperationContract]
        void MakeAnalogEntryToMeasurementCache(Dictionary<long, AnalogModbusData> data, bool permissionToPublishData);

        [OperationContract]
        void MakeDiscreteEntryToMeasurementCache(Dictionary<long, DiscreteModbusData> data, bool permissionToPublishData);
    }
}
