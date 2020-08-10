using Common.Web.Models.ViewModels;
using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.Contracts.WebAdapterContracts
{
    [ServiceContract]
    public interface IWebAdapterContract: IService
    {
        [OperationContract]
        Task UpdateGraph(List<NodeViewModel> nodes, List<RelationViewModel> relations);

        [OperationContract]
        Task UpdateScadaData(Dictionary<long, AnalogModbusData> scadaData);
    }
}
