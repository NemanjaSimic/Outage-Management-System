namespace OMS.Web.Common.Extensions
{
    using OMS.Web.UI.Models.ViewModels;

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
