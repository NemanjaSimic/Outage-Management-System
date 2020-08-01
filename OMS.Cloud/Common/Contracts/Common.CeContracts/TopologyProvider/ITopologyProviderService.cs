using CECommon.CeContrats;
using CECommon.Interface;
using CECommon.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.CeContracts.TopologyProvider
{
	[ServiceContract]
	public interface ITopologyProviderService : IService
	{
        [OperationContract]
        Task<List<ITopology>> GetTopologies();
        [OperationContract]
        Task CommitTransaction();
        [OperationContract]
        Task<bool> PrepareForTransaction();
        [OperationContract]
        Task RollbackTransaction();
        [OperationContract]
        Task<bool> IsElementRemote(long elementGid);
        [OperationContract]
        Task ResetRecloser(long recloserGid);
        [OperationContract]
        Task<IOutageTopologyModel> GetOMSModel();
        [OperationContract]
        Task<UIModel> GetTopology();
    }
}
