using Common.PubSubContracts.DataContracts.CE;
using System.Collections.Generic;
using System.ServiceModel;

namespace OMS.Common.PubSubContracts.Interfaces
{
    public delegate void SwitchClosed(long elementGid);
    public delegate void ConsumersBlackedOut(List<long> consumers, long? outageId);
    public delegate void SwitchOpened(long elementId, long? outageId);
    public delegate void ConsumersEnergized(HashSet<long> consumers);

    //public interface IOutageTopologyModel
    //{
    //    long FirstNode { get; set; }
    //    Dictionary<long, IOutageTopologyElement> OutageTopology { get; }
    //    void AddElement(IOutageTopologyElement newElement);
    //    bool GetElementByGid(long gid, out IOutageTopologyElement topologyElement);
    //    bool GetElementByGidFirstEnd(long gid, out IOutageTopologyElement topologyElement);
    //}
}
