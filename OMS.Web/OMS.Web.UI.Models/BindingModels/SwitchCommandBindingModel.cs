namespace OMS.Web.UI.Models.BindingModels
{
    using System.ComponentModel.DataAnnotations;

    public enum SwitchCommandType
    {
        TURN_OFF = 1,
        TURN_ON = 0
    }

    public class SwitchCommandBindingModel
    {
        [Required]
        public long Guid;

        [Required]
        public SwitchCommandType Command;
    }
}
