namespace OMS.Web.Common.Extensions
{
    using OMS.Web.UI.Models.ViewModels;
    
    public static class NodeExtensions
    {
        public static TransformerNodeViewModel ToTransformerNode(this NodeViewModel node)
            => new TransformerNodeViewModel
            {
                Id = node.Id,
                Mrid = node.Mrid,
                Name = node.Name,
                Description = node.Description,
                DMSType = node.DMSType,
                IsActive = node.IsActive,
                IsRemote = node.IsRemote,
                NoReclosing = node.NoReclosing,
                Measurements = node.Measurements,
                NominalVoltage = node.NominalVoltage,
                FirstWinding = new NodeViewModel(),
                SecondWinding = new NodeViewModel()
            };
    }
}
