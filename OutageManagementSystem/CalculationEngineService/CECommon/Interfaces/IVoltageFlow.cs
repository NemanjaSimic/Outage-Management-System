using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon.Interfaces
{
    public interface IVoltageFlow
    {
        void UpdateLoadFlow(List<ITopology> topologies);
    }
}
