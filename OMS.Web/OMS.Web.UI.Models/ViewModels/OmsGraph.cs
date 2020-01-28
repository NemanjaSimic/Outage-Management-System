
namespace OMS.Web.UI.Models.ViewModels
{
    using System.Collections.Generic;
    
    public class OmsGraph
    {
        public List<Node> Nodes;
        public List<Relation> Relations;

        public OmsGraph()
        {
            Nodes = new List<Node>();
            Relations = new List<Relation>();
        }
    }
}
