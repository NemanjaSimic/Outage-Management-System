using OMS.Common.SCADA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Common.ScadaContracts
{
    public interface IWriteCommandEnqueuer
    {
        Task<bool> EnqueueWriteCommand(IWriteModbusFunction modbusFunction);
    }
}
