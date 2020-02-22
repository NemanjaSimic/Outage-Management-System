namespace OMS.Web.UI.Models.ViewModels
{
    using System;
    using System.Collections.Generic;

    public class ArchivedOutageViewModel : OutageViewModel
    {
        public DateTime ArchivedAt { get; set; }

        public ArchivedOutageViewModel() => AffectedConsumers = new List<ConsumerViewModel>();
    }
}
