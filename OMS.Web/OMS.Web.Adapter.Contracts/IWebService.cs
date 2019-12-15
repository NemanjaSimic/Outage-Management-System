using OMS.Web.UI.Models;
using System.Collections.Generic;
using System.ServiceModel;

namespace OMS.Web.Adapter.Contracts
{
    [ServiceContract]
    public interface IWebService
    {
        [OperationContract]
        void UpdateGraph(List<Node> nodes, List<Relation> relations);
    }
}
