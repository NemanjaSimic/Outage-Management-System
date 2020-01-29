namespace OMS.Web.UI.Models.ViewModels
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Single item that will be present on the UI.
    /// Contains information about the element
    /// </summary>
    public class Node : IEquatable<Node>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Mrid { get; set; }
        public string Description { get; set; }
        public string DMSType { get; set; }
        public bool IsActive { get; set; }
        public string NominalVoltage { get; set; }
        public bool IsRemote { get; set; }

        public List<Measurement> Measurements { get; set; }

        public Node()
        {
            Measurements = new List<Measurement>();
        }

        public bool Equals(Node other)
            => Id == other.Id
            && Name == other.Name
            && Mrid == other.Mrid
            && Description == other.Description
            && DMSType == other.DMSType
            && IsActive == other.IsActive
            && NominalVoltage == other.NominalVoltage
            && IsRemote == other.IsRemote
            && Measurements.Equals(other.Measurements);   
    }
}
