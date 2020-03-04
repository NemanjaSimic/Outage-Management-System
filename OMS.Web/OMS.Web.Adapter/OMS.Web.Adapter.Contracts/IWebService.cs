namespace OMS.Web.Adapter.Contracts
{
    using OMS.Web.UI.Models.ViewModels;
    using Outage.Common.PubSub.SCADADataContract;
    using System.Collections.Generic;
    using System.ServiceModel;

    [ServiceContract]
    public interface IWebService
    {
        [OperationContract]
        void UpdateGraph(List<NodeViewModel> nodes, List<RelationViewModel> relations);

        [OperationContract]
        void UpdateScadaData(Dictionary<long, AnalogModbusData> scadaData);
    }
}
