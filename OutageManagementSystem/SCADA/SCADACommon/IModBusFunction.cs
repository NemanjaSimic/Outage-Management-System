using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.SCADA.SCADA_Common
{
    public interface IModBusFunction
    {
        Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] receivedBytes);

		byte[] PackRequest();
    }
}
