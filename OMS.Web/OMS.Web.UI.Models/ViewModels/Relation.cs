namespace OMS.Web.UI.Models.ViewModels
{
    /// <summary>
    /// Represents a parent-child relation between two nodes.
    /// Contains additional information about the relation.
    /// </summary>
    public class Relation
    {
        public string SourceNodeId;
        public string TargetNodeId;
        public bool IsActive;
        public bool IsAclLine; 
    }
}
