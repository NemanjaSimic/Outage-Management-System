using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OMS.OutageSimulator.UI.BindingModels
{
    public class ActiveOutageBindingModel
    {
        public GlobalIDBindingModel OutageElement { get; set; }
        public ObservableCollection<GlobalIDBindingModel> OptimumIsolationPoints { get; set; }
        public ObservableCollection<GlobalIDBindingModel> DefaultIsolationPoints { get; set; }
        public Dictionary<long, long> DefaultToOptimumIsolationPointMap { get; set; }

        public ActiveOutageBindingModel()
        {
            OptimumIsolationPoints = new ObservableCollection<GlobalIDBindingModel>();
            DefaultIsolationPoints = new ObservableCollection<GlobalIDBindingModel>();
            DefaultToOptimumIsolationPointMap = new Dictionary<long, long>();
        }
    }
}
