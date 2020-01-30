namespace OMS.Web.Common.Extensions
{
    using OMS.Web.UI.Models.ViewModels;
    
    public static class NodeExtensions
    {
        public static TransformerNode ToTransformerNode(this Node node)
            => new TransformerNode
            {
                Id = node.Id,
                Mrid = node.Mrid,
                Name = node.Name,
                Description = node.Description,
                DMSType = node.DMSType,
                IsActive = node.IsActive,
                IsRemote = node.IsRemote,
                Measurements = node.Measurements,
                NominalVoltage = node.NominalVoltage,
                FirstWinding = new Node(),
                SecondWinding = new Node()
            };
    }
}
