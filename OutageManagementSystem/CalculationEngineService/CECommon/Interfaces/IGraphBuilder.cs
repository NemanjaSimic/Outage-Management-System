using CECommon.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon.Interfaces
{
    public interface IGraphBuilder
    {
        Topology CreateGraphTopology(long firstElementGid);
    }
}
