using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.SCADA.SCADACommon.FunctionParameters
{
    public interface IModbusReadCommandParameters : IModbusCommandParameters
    {
        ushort StartAddress { get; }

        ushort Quantity { get; }
    }
}
