using EasyModbus;
using System;
using System.Collections.Generic;

namespace Outage.SCADA.SCADACommon
{
    public interface IModBusFunction
    {
        void Execute(ModbusClient modbusClient);
    }

    public interface IReadDigitalModBusFunction : IModBusFunction
    {
        Dictionary<long, bool> Data { get; }
    }

    public interface IReadAnalogModBusFunction : IModBusFunction
    {
        Dictionary<long, int> Data { get; }
    }
}