using Microsoft.ServiceFabric.Services.Remoting;
using Outage.Common.PubSub.SCADADataContract;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace OMS.Common.ScadaContracts
{
    [ServiceContract]
    public interface IScadaModelUpdateAccessContract : IService
    {
        [OperationContract]
        Task MakeAnalogEntryToMeasurementCache(Dictionary<long, AnalogModbusData> data, bool permissionToPublishData);

        [OperationContract]
        Task MakeDiscreteEntryToMeasurementCache(Dictionary<long, DiscreteModbusData> data, bool permissionToPublishData);
    }
}
