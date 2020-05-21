using Common.SCADA;
using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.ScadaContracts.DataContracts;
using OMS.Common.ScadaContracts.DataContracts.ScadaModelPointItems;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace OMS.Common.ScadaContracts.ModelProvider
{
    [ServiceContract]
    public interface IScadaModelReadAccessContract : IService
    {
        [OperationContract]
        Task<bool> GetIsScadaModelImportedIndicator();

        [OperationContract]
        [ServiceKnownType(typeof(ScadaConfigData))]
        Task<IScadaConfigData> GetScadaConfigData();

        [OperationContract]
        [ServiceKnownType(typeof(AnalogPointItem))]
        [ServiceKnownType(typeof(DiscretePointItem))]
        [ServiceKnownType(typeof(AlarmConfigData))]
        Task<Dictionary<long, IScadaModelPointItem>> GetGidToPointItemMap();

        [OperationContract]
        Task<Dictionary<short, Dictionary<ushort, long>>> GetAddressToGidMap();

        [OperationContract]
        [ServiceKnownType(typeof(AnalogPointItem))]
        [ServiceKnownType(typeof(DiscretePointItem))]
        [ServiceKnownType(typeof(AlarmConfigData))]
        Task<Dictionary<short, Dictionary<ushort, IScadaModelPointItem>>> GetAddressToPointItemMap();

        [OperationContract]
        Task<Dictionary<long, CommandDescription>> GetCommandDescriptionCache();
    }
}
