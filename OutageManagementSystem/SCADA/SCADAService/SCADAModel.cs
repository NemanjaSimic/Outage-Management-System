using Outage.Common;
using Outage.Common.GDA;
using Outage.SCADA.ModBus.Connection;
using Outage.SCADA.SCADA_Common;
using Outage.SCADA.SCADA_Config_Data.Configuration;
using Outage.SCADA.SCADA_Config_Data.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace Outage.SCADA.SCADAService
{
    public class SCADAModel
    {
        //TODO: stanje "modela" -> npr string putanje ka dokumentu
        private DataModelRepository scadaModel = DataModelRepository.Instance;
        private Dictionary<DeltaOpType, List<long>> modelChanges;
        private Dictionary<long, ConfigItem> delta_Points;
        private ModelResourcesDesc modelRD;
        private ConfigWriter ConfigWriter;
        public SCADAModel()
        {
            delta_Points = new Dictionary<long, ConfigItem>();

        }
        public bool Notify(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            this.modelChanges = modelChanges;
            return true;
        }

        public bool Prepare()
        {

            try
            {
                modelRD = new ModelResourcesDesc();

                foreach (var item in scadaModel.Points)
                {
                    delta_Points.Add(item.Key, (ConfigItem)item.Value.Clone());
                }

                foreach (var item in modelChanges[DeltaOpType.Delete])
                {
                    if (delta_Points.ContainsKey(item)) delta_Points.Remove(item);
                }

                foreach (var item in modelChanges[DeltaOpType.Insert])
                {
                    delta_Points.Add(item, ChangeOp(item));
                }

                foreach (var item in modelChanges[DeltaOpType.Update])
                {

                    delta_Points[item] = ChangeOp(item);

                }
                ConfigWriter = new ConfigWriter(scadaModel.ConfigFileName, delta_Points.Values.ToList());
      


            }
            catch (Exception)
            {

                return false;
            }


            return true;
        }

        public void Commit()
        {
            try
            {
                scadaModel.Points = delta_Points;
                //Move validan u backup folder
                File.Move(scadaModel.pathMdbSimCfg, scadaModel.pathToDeltaCfg);
                //Move u MdbSim folder
                File.Move(scadaModel.ConfigFileName, scadaModel.pathMdbSimCfg);
                Console.WriteLine("There has been changes in cfg file.Reopen MdbSim .");
                Console.ReadKey();
                modelChanges.Clear();
            }
            catch (Exception)
            {
                throw;

            }
        }

        public void Rollback()
        {
            //Move validan u backup folder
            File.Delete(scadaModel.pathMdbSimCfg);
            //Move u MdbSim folder
            File.Move(scadaModel.pathToDeltaCfg, scadaModel.pathMdbSimCfg);
            delta_Points.Clear();
            modelChanges.Clear();

        }
        public ConfigItem ChangeOp(long gid)
        {
            var type = modelRD.GetModelCodeFromId(gid);
            List<ModelCode> props;
            ResourceDescription rd;
            ConfigItem newCI;
            if (type == ModelCode.ANALOG)
            {
                props = new List<ModelCode>
                {
                ModelCode.IDOBJ_GID,
                ModelCode.IDOBJ_NAME,
                ModelCode.MEASUREMENT_ADDRESS,
                ModelCode.MEASUREMENT_ISINPUT,
                ModelCode.ANALOG_CURRENTVALUE,
                ModelCode.ANALOG_MAXVALUE,
                ModelCode.ANALOG_MINVALUE,
                ModelCode.ANALOG_NORMALVALUE
                };
                rd = scadaModel.gdaQueryProxy.GetValues(gid, props);
                newCI = scadaModel.ConfigurateConfigItem(rd.Properties, true);
            }
            else
            {
                props = new List<ModelCode>
                {
                ModelCode.IDOBJ_GID,
                ModelCode.IDOBJ_NAME,
                ModelCode.MEASUREMENT_ADDRESS,
                ModelCode.MEASUREMENT_ISINPUT,
                ModelCode.DISCRETE_CURRENTOPEN,
                ModelCode.DISCRETE_MAXVALUE,
                ModelCode.DISCRETE_MINVALUE,
                ModelCode.DISCRETE_NORMALVALUE
                };
                rd = scadaModel.gdaQueryProxy.GetValues(gid, props);
                newCI = scadaModel.ConfigurateConfigItem(rd.Properties, false);
            }
            return newCI;
        }
      
        
    }
}