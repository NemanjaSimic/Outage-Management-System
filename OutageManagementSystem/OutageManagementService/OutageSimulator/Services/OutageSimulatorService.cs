using OMS.OutageSimulator.UserControls;
using Outage.Common.ServiceContracts.OMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.OutageSimulator.Services
{
    public class OutageSimulatorService : IOutageSimulatorContract
    {
        public static Overview Overview { get; set; }

        public bool ResolvedOutage(long outageElementId)
        {
            return Overview.ResolveOutage(outageElementId);
        }
        public bool IsOutageElement(long outageElementId)
        {
            return Overview.ActiveOutages.Any(outage => outage.OutageElement.GID == outageElementId);
        }
    }
}
