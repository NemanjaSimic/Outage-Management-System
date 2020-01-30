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
      
        private Provider()
        {
            ModelResourcesDesc = new ModelResourcesDesc();
        }
        public ITopologyProvider TopologyProvider { get; set; } 
        public IModelProvider ModelProvider { get; set; }
        public ModelResourcesDesc ModelResourcesDesc { get; private set; }
        public IWebTopologyModelProvider WebTopologyModelProvider { get; set; }
        public ISCADAResultHandler SCADAResultHandler { get; set; }
        public ICacheProvider CacheProvider { get; set; }
    }
}
