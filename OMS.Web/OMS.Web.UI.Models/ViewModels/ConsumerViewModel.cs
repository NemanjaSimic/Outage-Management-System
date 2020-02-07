namespace OMS.Web.UI.Models.ViewModels
{
    using System.Collections.Generic;   
    
    public class ConsumerViewModel
    {
        public long Id { get; set; }

        public string Mrid { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public IEnumerable<ActiveOutageViewModel> ActiveOutages { get; set; }

        public IEnumerable<ArchivedOutageViewModel> ArchivedOutages { get; set; }


        public ConsumerViewModel()
        {
            ActiveOutages = new List<ActiveOutageViewModel>();
            ArchivedOutages = new List<ArchivedOutageViewModel>();
        }
    }
}
