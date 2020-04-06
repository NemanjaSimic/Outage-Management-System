using OMS.Common.SCADA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Common.ScadaContracts
{
    public interface IReadCommandEnqueuer
    {
        Task<bool> EnqueueReadCommand(IReadModbusFunction modbusFunction);
    }
}
