using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Cloud.SCADA.Common
{
    public interface IReadCommandEnqueuer
    {
        bool EnqueueReadCommand(IReadModbusFunction modbusFunction);
    }

    public interface IWriteCommandEnqueuer
    {
        bool EnqueueWriteCommand(IWriteModbusFunction modbusFunction);
    }

    public interface IModelUpdateCommandEnqueuer
    {
        bool EnqueueModelUpdateCommands(List<long> measurementGids);
    }
}
