namespace OMS.Web.UI.Models.ViewModels
{
    using System;
    using System.Collections.Generic;
    
    public class ArchivedOutageViewModel
    {
        public long Id { get; set; }
        public long ElementId { get; set; }
        public DateTime ReportedAt { get; set; }
        public DateTime ArchivedAt { get; set; }
        public IEnumerable<ConsumerViewModel> AffectedConsumers { get; set; }


        public ArchivedOutageViewModel() => AffectedConsumers = new List<ConsumerViewModel>();
    }
}
