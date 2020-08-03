using Common.Web.UI.Models.ViewModels;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using System.Collections.Generic;
using System.ServiceModel;

namespace Common.Contracts.WebAdapterContracts
{
    [ServiceContract]
    public interface IWebService
    {
        [OperationContract]
        void UpdateGraph(List<NodeViewModel> nodes, List<RelationViewModel> relations);

        [OperationContract]
        void UpdateScadaData(Dictionary<long, AnalogModbusData> scadaData);
    }
}
