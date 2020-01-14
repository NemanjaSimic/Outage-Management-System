using System.Collections.Generic;

namespace OMS.Web.UI.Models.ViewModels
{
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
