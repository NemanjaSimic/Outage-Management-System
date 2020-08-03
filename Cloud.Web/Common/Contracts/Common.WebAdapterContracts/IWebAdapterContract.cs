using Common.Web.Models.ViewModels;
using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using System.Collections.Generic;
using System.ServiceModel;

namespace Common.Contracts.WebAdapterContracts
{
    [ServiceContract]
    public interface IWebAdapterContract: IService
    {
        [OperationContract]
        void UpdateGraph(List<NodeViewModel> nodes, List<RelationViewModel> relations);

        [OperationContract]
        void UpdateScadaData(Dictionary<long, AnalogModbusData> scadaData);
    }
}
