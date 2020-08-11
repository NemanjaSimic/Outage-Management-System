using Common.CloudContracts;
using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.Cloud;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.OmsContracts.ModelProvider
{
    [ServiceContract]
    public interface IOutageModelUpdateAccessContract:IService, IHealthChecker
    {
        [OperationContract]
        Task UpdateCommandedElements(long gid, ModelUpdateOperationType modelUpdateOperationType);
        [OperationContract]
        Task UpdateOptimumIsolationPoints(long gid, ModelUpdateOperationType modelUpdateOperationType);
        [OperationContract]
        Task UpdatePotentialOutage(long gid, CommandOriginType commandOriginType, ModelUpdateOperationType modelUpdateOperationType);

    }
}
