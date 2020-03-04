namespace OMS.Web.UI.Models.ViewModels
{
    using System.Collections.Generic;

    public class ActiveOutageViewModel : OutageViewModel
    {
        public bool IsResolveConditionValidated { get; set; }
        public ActiveOutageLifecycleState State { get; set; }
        

        public ActiveOutageViewModel()
        {

        }
    }
}
