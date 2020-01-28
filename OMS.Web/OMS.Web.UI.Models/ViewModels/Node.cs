namespace OMS.Web.UI.Models.ViewModels
{
    using System.Collections.Generic;

    /// <summary>
    /// Single item that will be present on the UI.
    /// Contains information about the element
    /// </summary>
    public class Node
    {
        public string Id;
        public string Name;
        public string Mrid;
        public string Description;
        public string DMSType;
        public bool IsActive;
        public string NominalVoltage;
        public bool IsRemote;

        public List<Measurement> Measurements;

        public Node()
        {
            Measurements = new List<Measurement>();
        }
    }
}
