namespace OMS.Web.UI.Models
{
    /// <summary>
    /// Represents a parent-child relation between two nodes.
    /// Contains additional information about the relation.
    /// </summary>
    public class Relation
    {
        // These can be changed later on ...
        public string SourceNodeId;
        public string TargetNodeId;
        public bool IsActive;
        public bool IsAclLine; 
    }
}
