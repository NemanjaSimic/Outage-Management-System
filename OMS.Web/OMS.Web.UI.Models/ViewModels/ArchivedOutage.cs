using System;
using System.Collections.Generic;

namespace OMS.Web.UI.Models.ViewModels
{
    public class ArchivedOutage
    {
        public long Id { get; set; }
        public long ElementId { get; set; }
        public DateTime DateCreated { get; set; }
        public List<long> AfectedConsumers { get; set; }

        public ArchivedOutage()
        {
            AfectedConsumers = new List<long>();
        }

        //TODO: IF NEEDED 
        //public bool Equals(ArchivedOutage other)
        //    => base.Equals(other)
        //    && Id.Equals(other.Id)
        //    && ElementId.Equals(other.ElementId)
        //    && DateCreated.Equals(other.DateCreated)
        //    && AfectedConsumers.Equals(other.AfectedConsumers); //da li daporedimo referencu bas ili sve elemente pojedinacno
    }
}
