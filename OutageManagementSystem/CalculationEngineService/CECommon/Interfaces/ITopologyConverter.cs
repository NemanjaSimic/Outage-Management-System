using CECommon.Model;
using Outage.Common.OutageService.Interface;
using Outage.Common.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon.Interfaces
{
    public interface ITopologyConverter
    {
        IOutageTopologyModel ConvertTopologyToOMSModel(ITopology topology);
        UIModel ConvertTopologyToUIModel(ITopology topology);
    }
}
