using OMS.Common.PubSubContracts.Interfaces;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Common.PubSubContracts.DataContracts.CE
{
    [DataContract(IsReference = true)]
    public class OutageTopologyModel
    {
        private long firstNode;
        private Dictionary<long, OutageTopologyElement> outageTopology;
        [DataMember]
        public long FirstNode { get { return firstNode; } set { firstNode = value; } }
        [DataMember]
        public Dictionary<long, OutageTopologyElement> OutageTopology { get { return outageTopology; } set { outageTopology = value; } }

        public OutageTopologyModel()
        {
            OutageTopology = new Dictionary<long, OutageTopologyElement>();
        }

        public void AddElement(OutageTopologyElement newElement)
        {
            if (!OutageTopology.ContainsKey(newElement.Id))
            {
                OutageTopology.Add(newElement.Id, newElement);
            }
        }

        public bool GetElementByGid(long gid, out OutageTopologyElement topologyElement)
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

        public bool GetElementByGidFirstEnd(long gid, out OutageTopologyElement topologyElement)
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
