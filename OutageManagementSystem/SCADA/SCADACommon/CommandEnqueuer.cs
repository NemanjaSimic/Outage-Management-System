using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.SCADA.SCADACommon
{
    public interface IReadCommandEnqueuer
    {
        bool EnqueueReadCommand(IModbusFunction modbusFunction);
    }

    public interface IWriteCommandEnqueuer
    {
        bool EnqueueWriteCommand(IModbusFunction modbusFunction);
    }

    public interface IModelUpdateCommandEnqueuer
    {
        bool EnqueueModelUpdateCommands(List<long> measurementGids);
    }
}
