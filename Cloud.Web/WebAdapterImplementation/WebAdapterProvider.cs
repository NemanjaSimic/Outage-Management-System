using Common.Contracts.WebAdapterContracts;
using Common.Web.Models.ViewModels;
using System;
using System.Collections.Generic;

namespace WebAdapterImplementation
{
    public class WebAdapterProvider : IWebAdapterContract
    {
        public void UpdateGraph(List<NodeViewModel> nodes, List<RelationViewModel> relations)
        {
            throw new NotImplementedException();
        }

        public void UpdateScadaData(Dictionary<long, OMS.Common.PubSubContracts.DataContracts.SCADA.AnalogModbusData> scadaData)
        {
            throw new NotImplementedException();
        }
    }
}
