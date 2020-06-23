using Common.OMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.OmsContracts.DataContracts.OutageModel
{
    public class OutageTopologyModel
    {
        private long firstNode;
        private Dictionary<long, IOutageTopologyElement> outageTopology;
        [DataMember]
        public long FirstNode { get { return firstNode; } set { firstNode = value; } }
        [DataMember]
        public Dictionary<long, IOutageTopologyElement> OutageTopology { get { return outageTopology; } set { outageTopology = value; } }
        public OutageTopologyModel()
        {
            OutageTopology = new Dictionary<long, IOutageTopologyElement>();
        }

        public void AddElement(IOutageTopologyElement newElement)
        {
            if (!OutageTopology.ContainsKey(newElement.Id))
            {
                OutageTopology.Add(newElement.Id, newElement);
            }
        }

        public bool GetElementByGid(long gid, out IOutageTopologyElement topologyElement)
        {
            bool success = false;
            if (OutageTopology.TryGetValue(gid, out topologyElement))
            {
                success = true;
            }
            else
            {
                topologyElement = null;
            }
            return success;
        }

        public bool GetElementByGidFirstEnd(long gid, out IOutageTopologyElement topologyElement)
        {
            bool success = GetElementByGid(gid, out topologyElement);
            if (success)
            {
                success = GetElementByGid(topologyElement.FirstEnd, out topologyElement);
                if (!success) topologyElement = null;
            }
            else
            {
                topologyElement = null;
            }
            return success;

        }
    }
}
