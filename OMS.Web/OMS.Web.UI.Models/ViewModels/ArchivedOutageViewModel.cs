namespace OMS.Web.UI.Models.ViewModels
{    
    using System.Collections.Generic;

    public class ArchivedOutageViewModel : OutageViewModel
    {
        public ArchivedOutageViewModel() => AffectedConsumers = new List<ConsumerViewModel>();
    }
}
