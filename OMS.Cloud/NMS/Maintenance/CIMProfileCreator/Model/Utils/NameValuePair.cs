
namespace FTN.ESI.SIMES.CIM.Model.Utils
{
    public class NameValuePair
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public NameValuePair(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
}
