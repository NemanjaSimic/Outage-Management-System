using System.Collections.Generic;

namespace Outage.Common.OutageService.Interface
{
    public interface IOutageTopologyModel
    {
        long FirstNode { get; set; }
        Dictionary<long, IOutageTopologyElement> OutageTopology { get; }
        void AddElement(IOutageTopologyElement newElement);
        bool GetElementByGid(long gid, out IOutageTopologyElement topologyElement);
        bool GetElementByGidFirstEnd(long gid, out IOutageTopologyElement topologyElement);
    }
}
