using EasyModbus;
using Outage.Common;
using Outage.SCADA.ModBus.FunctionParameters;
using Outage.SCADA.SCADACommon;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Outage.SCADA.ModBus.ModbusFuntions
{
    public abstract class ModbusFunction : IModBusFunction
    {
        protected ILogger logger = LoggerWrapper.Instance;
        
        public ModbusCommandParameters CommandParameters { get; protected set; }

        protected ModbusFunction(ModbusCommandParameters commandParameters)
        {
            CommandParameters = commandParameters;
        }


        public override string ToString()
        {
            return $"Transaction: {CommandParameters.TransactionId}, command {CommandParameters.FunctionCode}";
        }

        protected void CheckArguments(MethodBase m, Type t)
        {
            if (CommandParameters.GetType() != t)
            {
                string message = $"{m.ReflectedType.Name}{m.Name} has invalid argument {nameof(CommandParameters)} of type {CommandParameters.GetType().Name}.{Environment.NewLine}Argumet type should be {t.Name}";
                throw new ArgumentException(message);
            }
        }


        #region IModBusFunction

        public abstract void Execute(ModbusClient modbusClient);
        #endregion


        #region Obsolete
        /// <summary>
        /// Method is called from communication thread:
        /// Converts command parameters to byte array
        /// Parameters should be packed according to this:
        ///
        ///	 Name			| TransactionId | ProtocolId | Length | UnidId | Function Code | Start Address | Quantity |
        ///	 Type			|     ushort    |   ushort   | ushort |  byte  |      byte     |    ushort     |  ushort  |
        ///	 SizeOF(type)	|        2      |      2     |   2    |   1    |        1      |       2       |     2    |
        ///
        /// </summary>
        /// <returns>Command parameters in form of byte array</returns>
        [Obsolete]
        public abstract byte[] PackRequest();

        /// <summary>
        /// Method is called from communication thread:
        /// Converts received message to key-value pairs
        /// Response is packed according to this:
        ///
        ///	 Name			| TransactionId | ProtocolId | Length | UnidId | Function Code | Byte Count |               Data               |
        ///	 Type			|     ushort    |   ushort   | ushort |  byte  |      byte     |    byte    |             byte array           |
        ///	 SizeOF(type)	|        2      |      2     |   2    |   1    |        1      |      1     |   value from Byte Count field    |
        ///
        /// </summary>
        /// <param name="response">Message read form socket</param>
        /// <returns>
        ///		Dictionary that maps tuple to received value from MdbSim:
        ///		Key: Tuple<PointType, ushort> - complex key of point. Points unique identifier
        ///				- PointType - type of point
        ///				- Point address
        ///		Value: Value received from MdbSim
        /// </returns>
        [Obsolete]
        public abstract Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response);
        #endregion
    }
}