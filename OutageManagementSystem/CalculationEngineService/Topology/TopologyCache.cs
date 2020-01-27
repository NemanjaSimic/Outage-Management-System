using CECommon.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topology
{
    public class TopologyCache
    {
		public List<ITopology> TopologyModel { get; private set; }
		private TopologyCache()
        {
			TopologyModel = new List<ITopology>();
		}
		#region Singleton
		private static object syncObj = new object();
		private static TopologyCache instance;

		public static TopologyCache Instance
		{
			get
			{
				lock (syncObj)
				{
					if (instance == null)
					{
						instance = new TopologyCache();
					}
				}
				return instance;
			}
		}
		#endregion


	}
}
