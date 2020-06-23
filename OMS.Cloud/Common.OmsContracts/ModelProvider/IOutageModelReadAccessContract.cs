using Common.OMS;
using Common.OmsContracts.DataContracts.OutageModel;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.OmsContracts.ModelProvider
{
    [ServiceContract]
    public interface IOutageModelReadAccessContract : IService
    {

        [OperationContract]
        [ServiceKnownType(typeof(OutageTopologyModel))]
        Task<IOutageTopologyModel> GetTopologyModel();

    }
}
