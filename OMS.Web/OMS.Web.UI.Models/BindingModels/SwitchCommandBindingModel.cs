namespace OMS.Web.UI.Models.BindingModels
{
    using System.ComponentModel.DataAnnotations;

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
