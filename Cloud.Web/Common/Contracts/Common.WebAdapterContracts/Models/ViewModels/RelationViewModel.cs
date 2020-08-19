using System;

namespace Common.Web.Models.ViewModels
{
    /// <summary>
    /// Represents a parent-child relation between two nodes.
    /// Contains additional information about the relation.
    /// </summary>
    public class RelationViewModel : IEquatable<RelationViewModel>
    {
        public string SourceNodeId { get; set; }
        public string TargetNodeId { get; set; }
        public bool IsActive { get; set; }
        public bool IsAclLine { get; set; }

        public bool Equals(RelationViewModel other)
            => SourceNodeId == other.SourceNodeId
            && TargetNodeId == other.TargetNodeId
            && IsActive == other.IsActive
            && IsAclLine == other.IsAclLine;
    }
}
