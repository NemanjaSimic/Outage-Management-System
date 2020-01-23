using CECommon.Model;
using Outage.Common.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon.Interfaces
{
    public interface IWebTopologyBuilder
    {
        UIModel CreateTopologyForWeb(ITopology topology);
    }
}
