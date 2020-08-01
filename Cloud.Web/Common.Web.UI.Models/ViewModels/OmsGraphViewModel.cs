using System;
using System.Collections.Generic;

namespace Common.Web.UI.Models.ViewModels
{
    public class OmsGraphViewModel : IViewModel , IEquatable<OmsGraphViewModel>
    {
        public List<NodeViewModel> Nodes;
        public List<RelationViewModel> Relations;

        public OmsGraphViewModel()
        {
            Nodes = new List<NodeViewModel>();
            Relations = new List<RelationViewModel>();
        }

        public bool Equals(OmsGraphViewModel other)
            => Nodes.Equals(other.Nodes)
            && Relations.Equals(other.Relations);
    }
}
