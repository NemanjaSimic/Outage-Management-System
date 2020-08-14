using Common.CloudContracts;
using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.Cloud.AzureStorageHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.OmsContracts.TracingAlgorithm
{
    [ServiceContract]
    public interface ITracingAlgorithmContract:IService, IHealthChecker
    {

        [OperationContract]
        Task StartTracingAlgorithm(List<long> calls);
    }
}
