namespace OMS.Web.UI.Models.ViewModels
{
    using System.Collections.Generic;

    public class ActiveOutageViewModel : OutageViewModel
    {
        public bool IsValidated { get; set; }
        public OutageLifecycleState State { get; set; }
        

        public ActiveOutageViewModel()
        {

        }
    }
}
