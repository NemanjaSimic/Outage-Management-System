
namespace FTN.ESI.SIMES.CIM.Model.Utils
{
    public class PredefinedProperty
    {
        public string URI { get; set; }
        public string type { get; set; }

        public PredefinedProperty(string URI, string type)
        {
            this.URI = URI;
            this.type = type;
        }
    }
}
