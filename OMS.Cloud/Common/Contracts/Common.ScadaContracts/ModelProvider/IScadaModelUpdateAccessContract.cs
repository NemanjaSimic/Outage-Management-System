using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using OMS.Common.ScadaContracts.DataContracts;
using OMS.Common.ScadaContracts.DataContracts.ScadaModelPointItems;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace OMS.Common.ScadaContracts.ModelProvider
{
    [ServiceContract]
    public interface IScadaModelUpdateAccessContract : IService
    {
        [OperationContract]
        Task MakeAnalogEntryToMeasurementCache(Dictionary<long, AnalogModbusData> data, bool permissionToPublishData);

        [OperationContract]
        Task MakeDiscreteEntryToMeasurementCache(Dictionary<long, DiscreteModbusData> data, bool permissionToPublishData);

        [OperationContract]
        [ServiceKnownType(typeof(AnalogPointItem))]
        [ServiceKnownType(typeof(DiscretePointItem))]
        [ServiceKnownType(typeof(AlarmConfigData))]
        Task<IScadaModelPointItem> UpdatePointItemRawValue(long gid, int rawValue);

        [OperationContract]
        Task AddOrUpdateCommandDescription(long gid, CommandDescription commandDescription);

        [OperationContract]
        Task<bool> RemoveCommandDescription(long gid);
    }
}
