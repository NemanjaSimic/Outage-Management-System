namespace OMS.Web.UI.Models.ViewModels
{
    using System.Collections.Generic;

    public class ActiveOutageViewModel : OutageViewModel
    {
        public OutageLifecycleState State { get; set; }
        public IEnumerable<long> DefaultIsolationPoints { get; set; }
        public IEnumerable<long> OptimalIsolationPoints { get; set; }

        public ActiveOutageViewModel()
        {
            DefaultIsolationPoints = new List<long>();
            OptimalIsolationPoints = new List<long>();
        }
    }
}
