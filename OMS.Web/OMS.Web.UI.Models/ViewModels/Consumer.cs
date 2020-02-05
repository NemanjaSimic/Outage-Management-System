using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Web.UI.Models.ViewModels
{
    public class Consumer
    {
     
        public long ConsumerId { get; set; }

        public string ConsumerMRID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public List<ArchivedOutage> ArchivedOutages { get; set; }

        //public List<ActiveOutage> ActiveOutages { get; set; }

        public Consumer()
        {
            ArchivedOutages = new List<ArchivedOutage>();
        }
    }
}
