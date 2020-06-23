using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.OMS
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
