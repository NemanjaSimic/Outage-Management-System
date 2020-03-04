using EasyModbus;
using Outage.Common;
using Outage.Common.PubSub.SCADADataContract;
using Outage.SCADA.SCADACommon.FunctionParameters;
using System.Collections.Generic;

namespace Outage.SCADA.SCADACommon
{
    public interface IModbusFunction
    {
        void Execute(ModbusClient modbusClient);
    }

    public interface IWriteModbusFunction : IModbusFunction
    {
        CommandOriginType CommandOrigin { get; }
        IModbusWriteCommandParameters ModbusWriteCommandParameters { get; }
    }

    public interface IReadModbusFunction : IModbusFunction
    {
        IModbusReadCommandParameters ModbusReadCommandParameters { get; }
    }

    public interface IReadAnalogModusFunction : IReadModbusFunction
    {
        Dictionary<long, AnalogModbusData> Data { get; }
    }

    public interface IReadDiscreteModbusFunction : IReadModbusFunction
    {
        Dictionary<long, DiscreteModbusData> Data { get; }
    }
}