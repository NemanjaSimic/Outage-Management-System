namespace OMS.Web.UI.Models.ViewModels
{
    using System;

    /// <summary>
    /// Represents a parent-child relation between two nodes.
    /// Contains additional information about the relation.
    /// </summary>
    public class Relation : IEquatable<Relation>
    {
        public string SourceNodeId;
        public string TargetNodeId;
        public bool IsActive;
        public bool IsAclLine;

        public bool Equals(Relation other)
            => SourceNodeId == other.SourceNodeId
            && TargetNodeId == other.TargetNodeId
            && IsActive == other.IsActive
            && IsAclLine == other.IsAclLine;
    }
}
