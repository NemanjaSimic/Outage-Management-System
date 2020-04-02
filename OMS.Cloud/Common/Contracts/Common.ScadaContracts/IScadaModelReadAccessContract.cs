using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.SCADA;
using System.Collections.Generic;
using System.ServiceModel;

namespace OMS.Common.ScadaContracts
{
    [ServiceContract]
    public interface IScadaModelReadAccessContract : IService
    {
        [OperationContract]
        bool GetIsScadaModelImportedIndicator();

        [OperationContract]
        ISCADAConfigData GetScadaConfigData();

        [OperationContract]
        Dictionary<long, ISCADAModelPointItem> GetGidToPointItemMap();

        [OperationContract]
        Dictionary<PointType, Dictionary<ushort, long>> GetAddressToGidMap();

        [OperationContract]
        Dictionary<PointType, Dictionary<ushort, ISCADAModelPointItem>> GetAddressToPointItemMap();

        [OperationContract]
        Dictionary<long, CommandDescription> GetCommandDescriptionCache();
    }
}
