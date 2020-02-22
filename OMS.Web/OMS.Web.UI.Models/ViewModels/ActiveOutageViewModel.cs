namespace OMS.Web.UI.Models.ViewModels
{
    using System.Collections.Generic;

    public class ActiveOutageViewModel : OutageViewModel
    {
        public OutageLifecycleState State { get; set; }

        public ActiveOutageViewModel() => AffectedConsumers = new List<ConsumerViewModel>();
    }
}
