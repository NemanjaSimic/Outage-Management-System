using Outage.Common.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon.Interfaces
{
    public delegate void WebTopologyModelProviderDelegate(List<UIModel> uiModels);
    public interface IWebTopologyModelProvider
    {
        WebTopologyModelProviderDelegate WebTopologyModelProviderDelegate { get; set; }

        List<UIModel> GetUIModels();
    }
}
