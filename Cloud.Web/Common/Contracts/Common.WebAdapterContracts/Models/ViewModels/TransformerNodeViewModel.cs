using System;

namespace Common.Web.Models.ViewModels
{
    public class TransformerNodeViewModel : NodeViewModel, IEquatable<TransformerNodeViewModel>
    {
        public NodeViewModel FirstWinding { get; set; }
        public NodeViewModel SecondWinding { get; set; }

        public bool Equals(TransformerNodeViewModel other)
            => base.Equals(other)
            && FirstWinding.Equals(other.FirstWinding)
            && SecondWinding.Equals(other.SecondWinding);
    }


}
