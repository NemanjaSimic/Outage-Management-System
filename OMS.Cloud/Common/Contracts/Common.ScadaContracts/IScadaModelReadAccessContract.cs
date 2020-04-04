using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.SCADA;
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
        Task<ISCADAConfigData> GetScadaConfigData();

        [OperationContract]
        Task<Dictionary<long, ISCADAModelPointItem>> GetGidToPointItemMap();

        [OperationContract]
        Task<Dictionary<ushort, Dictionary<ushort, long>>> GetAddressToGidMap();

        [OperationContract]
        Task<Dictionary<ushort, Dictionary<ushort, ISCADAModelPointItem>>> GetAddressToPointItemMap();

        [OperationContract]
        Task<Dictionary<long, CommandDescription>> GetCommandDescriptionCache();
    }
}
