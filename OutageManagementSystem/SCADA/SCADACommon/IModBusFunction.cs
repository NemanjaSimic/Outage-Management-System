using System;
using System.Collections.Generic;

namespace Outage.SCADA.SCADA_Common
{
    public interface IModBusFunction
    {
        Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] receivedBytes);

        byte[] PackRequest();
    }
}