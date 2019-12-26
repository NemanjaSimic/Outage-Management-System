using CECommon.Interfaces;
using CECommon.Model;
using NetworkModelServiceFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopologyBuilder;

namespace Topology
{
    public class Topology
    {
        private ITopologyBuilder topologyBuilder;
        private List<long> roots;
        public TopologyModel TopologyModel { get; private set; }

        #region Singleton
        private static Topology instance;
        private static object syncObj = new object();
        private Topology()
        {
            topologyBuilder = new GraphBuilder();
        }

        public static Topology Instance
        {
            get 
            {
                lock (syncObj)
                {
                    if (instance == null)
                    {
                        instance = new Topology();
                    }
                    return instance;
                }
            }
        }
        #endregion


        public void UpdateTopology()
        {
            roots = GDAModelHelper.Instance.GetAllEnergySources();
            GDAModelHelper.Instance.RetrieveAllElements();
            TopologyModel = topologyBuilder.CreateGraphTopology(roots.First());
        }
    }
}
