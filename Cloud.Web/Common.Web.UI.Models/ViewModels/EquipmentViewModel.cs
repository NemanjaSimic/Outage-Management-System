using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Web.UI.Models.ViewModels
{
    public class EquipmentViewModel
    {
        public long Id { get; set; }
        public string Mrid { get; set; }

        public IEnumerable<ActiveOutageViewModel> ActiveOutages { get; set; }
        public IEnumerable<ArchivedOutageViewModel> ArchivedOutages { get; set; }

        public EquipmentViewModel()
        {
            ActiveOutages = new List<ActiveOutageViewModel>();
            ArchivedOutages = new List<ArchivedOutageViewModel>();
        }
    }
}
