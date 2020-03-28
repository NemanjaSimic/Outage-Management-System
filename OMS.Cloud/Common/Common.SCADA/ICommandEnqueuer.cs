using System.Collections.Generic;

namespace OMS.Common.SCADA
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
