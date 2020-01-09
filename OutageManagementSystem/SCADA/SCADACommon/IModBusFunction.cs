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
        bool[] Data { get; }
    }

    public interface IReadAnalogModBusFunction : IModBusFunction
    {
        int[] Data { get; }
    }
}