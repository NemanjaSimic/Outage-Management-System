namespace OMS.Web.UI.Models.ViewModels
{
    using System;

    public class TransformerNode : Node, IEquatable<TransformerNode>
    {
        public Node FirstWinding { get; set; }
        public Node SecondWinding { get; set; }

        public bool Equals(TransformerNode other)
            => base.Equals(other)
            && FirstWinding.Equals(other.FirstWinding)
            && SecondWinding.Equals(other.SecondWinding);
    }


}
