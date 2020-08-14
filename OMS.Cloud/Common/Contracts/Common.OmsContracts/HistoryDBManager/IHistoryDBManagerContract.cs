using Common.CloudContracts;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.OmsContracts.HistoryDBManager
{
    [ServiceContract]
    public interface IHistoryDBManagerContract : IService, IHealthChecker
    {
        [OperationContract]
        Task OnSwitchClosed(long elementGid);
        [OperationContract]
        Task OnConsumerBlackedOut(List<long> consumers, long? outageId);
        [OperationContract]
        Task OnSwitchOpened(long elementGid, long? outageId);
        [OperationContract]
        Task OnConsumersEnergized(HashSet<long> consumers);
    }
}
