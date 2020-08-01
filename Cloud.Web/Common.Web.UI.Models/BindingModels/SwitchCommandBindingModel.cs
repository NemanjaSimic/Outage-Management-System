using System.ComponentModel.DataAnnotations;

namespace Common.Web.UI.Models.BindingModels
{
    public enum SwitchCommandType
    {
        OPEN = 1,
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
