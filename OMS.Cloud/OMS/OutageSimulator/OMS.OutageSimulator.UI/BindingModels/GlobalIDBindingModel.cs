using OMS.Common.Cloud;
using OMS.Common.NmsContracts;

namespace OMS.OutageSimulator.UI.BindingModels
{
    public class GlobalIDBindingModel
    {
        public long GID { get; set; }

        public string Type { get; set; }

        public GlobalIDBindingModel(long gid)
        {
            GID = gid;
            Type = ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(gid)).ToString();
        }
    }
}
