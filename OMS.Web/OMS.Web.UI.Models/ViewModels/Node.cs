namespace OMS.Web.UI.Models.ViewModels
{
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
        public string MeasurementValue;
        public string MeasurementType;
        public bool IsActive;
        public string NominalVoltage;
        public bool IsRemote;
    }
}
