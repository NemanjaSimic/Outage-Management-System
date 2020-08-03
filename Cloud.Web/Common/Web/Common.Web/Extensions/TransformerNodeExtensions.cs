using Common.Web.Models.ViewModels;

namespace Common.Web.Extensions
{
    public static class TransformerNodeExtensions
    {
        public static TransformerNodeViewModel AddFirstWinding(this TransformerNodeViewModel transformer, NodeViewModel winding)
        {
            transformer.FirstWinding = winding;
            return transformer;
        }

        public static TransformerNodeViewModel AddSecondWinding(this TransformerNodeViewModel transformer, NodeViewModel winding)
        {
            transformer.SecondWinding = winding;
            return transformer;
        }
    }
}
