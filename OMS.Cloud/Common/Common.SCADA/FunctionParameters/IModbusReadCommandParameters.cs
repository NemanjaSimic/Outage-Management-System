using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Common.SCADA.FunctionParameters
{
    public interface IModbusReadCommandParameters : IModbusCommandParameters
    {
        ushort StartAddress { get; }

        ushort Quantity { get; }
    }
}
