using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.OutageSimulator.BindingModels
{
    public class ActiveOutageBindingModel
    {
        public GlobalIDBindingModel OutageElement { get; set; }
        public List<GlobalIDBindingModel> OptimumIsolationPoints { get; set; }
        public List<GlobalIDBindingModel> DefaultIsolationPoints { get; set; }
        public Dictionary<long, long> DefaultToOptimumIsolationPointMap { get; set; }

        public ActiveOutageBindingModel()
        {
            OptimumIsolationPoints = new List<GlobalIDBindingModel>();
            DefaultIsolationPoints = new List<GlobalIDBindingModel>();
            DefaultToOptimumIsolationPointMap = new Dictionary<long, long>();
        }
    }
}
