using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Common.Web.Models.BindingModels
{
    [DataContract]
    public enum SwitchCommandType
    {
        [EnumMember]
        OPEN = 1,
        [EnumMember]
        CLOSE = 0,
    }

    public class SwitchCommandBindingModel
    {
        [Required]
        public long Guid;

        [Required]
        public SwitchCommandType Command;
    }
}
