using Common.CloudContracts;
using Common.PubSubContracts.DataContracts.CE;
using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.Cloud;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.OmsContracts.ModelProvider
{
    [ServiceContract]
    public interface IOutageModelReadAccessContract : IService, IHealthChecker
    {
        [OperationContract]
        [ServiceKnownType(typeof(OutageTopologyModel))]
        Task<OutageTopologyModel> GetTopologyModel();

        [OperationContract]
        Task<Dictionary<long, long>> GetCommandedElements();
        
        [OperationContract]
        Task<Dictionary<long, long>> GetOptimumIsolatioPoints();

        [OperationContract]
        [ServiceKnownType(typeof(OutageTopologyElement))]
        Task<OutageTopologyElement> GetElementById(long gid);
    }
}
