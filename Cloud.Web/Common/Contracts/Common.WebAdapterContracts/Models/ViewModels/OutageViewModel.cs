using System;
using System.Collections.Generic;

namespace Common.Web.Models.ViewModels
{
    public class OutageViewModel : IViewModel
    {
        public long Id { get; set; }
        public DateTime ReportedAt { get; set; }
        public DateTime? IsolatedAt { get; set; }
        public DateTime? RepairedAt { get; set; }
        public long ElementId { get; set; }
        public OutageLifecycleState State { get; set; }
        public IEnumerable<EquipmentViewModel> DefaultIsolationPoints { get; set; }
        public IEnumerable<EquipmentViewModel> OptimalIsolationPoints { get; set; }
        public IEnumerable<ConsumerViewModel> AffectedConsumers { get; set; }

        public OutageViewModel()
        {
            DefaultIsolationPoints = new List<EquipmentViewModel>();
            OptimalIsolationPoints = new List<EquipmentViewModel>();
            AffectedConsumers = new List<ConsumerViewModel>();
        }
    }
}
