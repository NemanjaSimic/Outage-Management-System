namespace OMS.Web.Common.Extensions
{
    using OMS.Web.UI.Models.ViewModels;

    public static class TransformerNodeExtensions
    {
        public static TransformerNode AddFirstWinding(this TransformerNode transformer, Node winding)
        {
            transformer.FirstWinding = winding;
            return transformer;
        }

        public static TransformerNode AddSecondWinding(this TransformerNode transformer, Node winding)
        {
            transformer.SecondWinding = winding;
            return transformer;
        }
    }
}
