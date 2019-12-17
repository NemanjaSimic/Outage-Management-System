using OMS.Web.Common.Constants;
using System.ComponentModel.DataAnnotations;

namespace OMS.Web.UI.Models.BindingModels
{
    public class SwitchCommand
    {
        [Required]
        public long Guid;

        [Required]
        public SwitchCommandType Command;
    }
}
