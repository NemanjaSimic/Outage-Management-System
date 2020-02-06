namespace OMS.Web.UI.Models.ViewModels
{
    using System;
    using System.Collections.Generic;
    
    public class ArchivedOutageViewModel
    {
        public long Id { get; set; }
        public long ElementId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ConsumerViewModel> AfectedConsumers { get; set; }

        public ArchivedOutageViewModel() => AfectedConsumers = new List<ConsumerViewModel>();
    }
}
