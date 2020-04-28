using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.ScadaContracts.DataContracts;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace OMS.Common.ScadaContracts
{
    [ServiceContract]
    public interface IScadaModelReadAccessContract : IService
    {
        [OperationContract]
        Task<bool> GetIsScadaModelImportedIndicator();

        [OperationContract]
        Task<ScadaConfigData> GetScadaConfigData();

        [OperationContract]
        [ServiceKnownType(typeof(AnalogPointItem))]
        [ServiceKnownType(typeof(DiscretePointItem))]
        Task<Dictionary<long, IScadaModelPointItem>> GetGidToPointItemMap();

        [OperationContract]
        Task<Dictionary<ushort, Dictionary<ushort, long>>> GetAddressToGidMap();

        [OperationContract]
        [ServiceKnownType(typeof(AnalogPointItem))]
        [ServiceKnownType(typeof(DiscretePointItem))]
        Task<Dictionary<ushort, Dictionary<ushort, IScadaModelPointItem>>> GetAddressToPointItemMap();

        [OperationContract]
        Task<Dictionary<long, CommandDescription>> GetCommandDescriptionCache();
    }
}
