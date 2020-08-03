using System;

namespace Common.Web.UI.Models.ViewModels
{
    /// <summary>
    /// Represents a parent-child relation between two nodes.
    /// Contains additional information about the relation.
    /// </summary>
    public class RelationViewModel : IEquatable<RelationViewModel>
    {
        public string SourceNodeId;
        public string TargetNodeId;
        public bool IsActive;
        public bool IsAclLine;

        public bool Equals(RelationViewModel other)
            => SourceNodeId == other.SourceNodeId
            && TargetNodeId == other.TargetNodeId
            && IsActive == other.IsActive
            && IsAclLine == other.IsAclLine;
    }
}
