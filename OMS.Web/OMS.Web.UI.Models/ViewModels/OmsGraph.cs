namespace OMS.Web.UI.Models.ViewModels
{
    using System;
    using System.Collections.Generic;
    
    public class OmsGraph : IEquatable<OmsGraph>
    {
        public List<Node> Nodes;
        public List<Relation> Relations;

        public OmsGraph()
        {
            Nodes = new List<Node>();
            Relations = new List<Relation>();
        }

        public bool Equals(OmsGraph other)
            => Nodes.Equals(other.Nodes)
            && Relations.Equals(other.Relations);
    }
}
