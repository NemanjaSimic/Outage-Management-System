using EasyModbus;
using Outage.Common.PubSub.SCADADataContract;
using System.Collections.Generic;

namespace Outage.SCADA.SCADACommon
{
    public interface IModbusFunction
    {
        void Execute(ModbusClient modbusClient);
    }

    public interface IReadAnalogModusFunction : IModbusFunction
    {
        Dictionary<long, AnalogModbusData> Data { get; }
    }

    public interface IReadDiscreteModbusFunction : IModbusFunction
    {
        Dictionary<long, DiscreteModbusData> Data { get; }
    }
}