using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.SCADA;
using System.Collections.Generic;
using System.ServiceModel;

namespace OMS.Common.ScadaContracts
{
    [ServiceContract]
    public interface IScadaModelAccessContract : IService
    {
        [OperationContract]
        Dictionary<PointType, Dictionary<ushort, long>> GetAddressToGidMapping();

        ISCADAConfigData GetScadaConfigData();
    }
}
