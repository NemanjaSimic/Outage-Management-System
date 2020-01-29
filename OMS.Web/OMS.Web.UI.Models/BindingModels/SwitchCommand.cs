namespace OMS.Web.UI.Models.BindingModels
{
    using System.ComponentModel.DataAnnotations;

    public enum SwitchCommandType
    {
        TURN_OFF = 0,
        TURN_ON = 1
    }

    public class SwitchCommand
    {
        [Required]
        public long Guid;

        [Required]
        public SwitchCommandType Command;
    }
}
