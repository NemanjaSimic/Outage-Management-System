using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.SCADA.SCADACommon
{
    public delegate bool ModelUpdateDelegate(List<long> modelUpdateGids);
}
