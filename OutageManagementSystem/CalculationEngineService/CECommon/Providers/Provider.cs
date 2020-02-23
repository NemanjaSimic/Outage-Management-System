using CECommon.Interfaces;
using Outage.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon.Providers
{
    public class Provider
    {
        #region Singleton
        private static Provider instance;
        private static object syncObj = new object();
        public static Provider Instance
        {
            get
            {
                lock (syncObj)
                {
                    if (instance == null)
                    {
                        instance = new Provider();
                    }
                }
                return instance;
            }
        }
        #endregion
        private Provider()
        {
            //ModelResourcesDesc = new ModelResourcesDesc();
        }
        public ITopologyProvider TopologyProvider { get; set; } 
        public IModelProvider ModelProvider { get; set; }
        public ModelResourcesDesc ModelResourcesDesc { get; private set; }
        public ITopologyConverterProvider TopologyConverterProvider { get; set; }
        public ISCADAResultHandler SCADAResultHandler { get; set; }
        public IMeasurementProvider MeasurementProvider { get; set; }
    }
}
