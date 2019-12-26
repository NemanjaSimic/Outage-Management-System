using Outage.Common.GDA;
using Outage.SCADA.SCADA_Config_Data.Configuration;
using Outage.SCADA.SCADA_Config_Data.Repository;
using System;
using System.Collections.Generic;

namespace Outage.SCADA.SCADAService
{
    public class SCADAModel
    {
        //TODO: stanje "modela" -> npr string putanje ka dokumentu
        private DataModelRepository scadaModel = DataModelRepository.Instance;
        private Dictionary<DeltaOpType, List<long>> modelChanges;
        private Dictionary<long, ResourceDescription> delta_NMS_Model;
        private Dictionary<long, ConfigItem> delta_Points;
        public bool Notify(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            this.modelChanges = modelChanges;
            return true;
        }

        public bool Prepare()
        {
            //TO DO:get inserted and updated RD and remove removed from collections
            delta_NMS_Model = new Dictionary<long, ResourceDescription>(scadaModel.NMS_Model);
            delta_Points = new Dictionary<long, ConfigItem>(scadaModel.Points);

            
            throw new NotImplementedException();
        }

        public void Commit()
        {
            throw new NotImplementedException();
        }

        public void Rollback()
        {
            throw new NotImplementedException();
        }
    }
}