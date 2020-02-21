namespace OMS.Web.UI.Models.ViewModels
{
    using System;
    using System.Collections.Generic;

    public class OutageViewModel : IViewModel
    {
        public long Id { get; set; }
        public long ElementId { get; set; }
        public DateTime ReportedAt { get; set; }
        public IEnumerable<ConsumerViewModel> AffectedConsumers { get; set; }

        public OutageViewModel() => AffectedConsumers = new List<ConsumerViewModel>();
    }
}
