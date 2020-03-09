namespace OMS.Web.UI.Models.ViewModels
{
    using System;
    using System.Collections.Generic;

    public class OutageViewModel : IViewModel
    {
        public long Id { get; set; }
        public DateTime ReportedAt { get; set; }
        public DateTime? IsolatedAt { get; set; }
        public DateTime? RepairedAt { get; set; }
        public long ElementId { get; set; }
        //public IEnumerable<long> DefaultIsolationPoints { get; set; }
        //public IEnumerable<long> OptimalIsolationPoints { get; set; }
        //TODO: uncomment when using EquipmentViewModel
        public IEnumerable<EquipmentViewModel> DefaultIsolationPoints { get; set; }
        public IEnumerable<EquipmentViewModel> OptimalIsolationPoints { get; set; }
        public IEnumerable<ConsumerViewModel> AffectedConsumers { get; set; }

        public OutageViewModel()
        {
            //DefaultIsolationPoints = new List<long>();
            //OptimalIsolationPoints = new List<long>();
            DefaultIsolationPoints = new List<EquipmentViewModel>();
            OptimalIsolationPoints = new List<EquipmentViewModel>();
            AffectedConsumers = new List<ConsumerViewModel>();
        }
    }
}
