using EasyModbus;
using Outage.Common;
using Outage.Common.PubSub.SCADADataContract;
using OMS.Cloud.SCADA.Common.FunctionParameters;
using System.Collections.Generic;

namespace OMS.Cloud.SCADA.Common
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