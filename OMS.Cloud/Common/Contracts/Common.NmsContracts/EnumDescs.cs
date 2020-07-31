using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using System;
using System.Collections.Generic;

namespace OMS.Common.NmsContracts
{
    public class EnumDescs
    {
        private readonly string baseLogString;

        private Dictionary<ModelCode, Type> property2enumType = new Dictionary<ModelCode, Type>();

        #region Private Properties
        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
        #endregion Private Properties

        public EnumDescs()
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            property2enumType.Add(ModelCode.ANALOG_SIGNALTYPE, typeof(AnalogMeasurementType));
            property2enumType.Add(ModelCode.DISCRETE_MEASUREMENTTYPE, typeof(DiscreteMeasurementType));
        }

        public List<string> GetEnumList(ModelCode propertyId)
        {
            List<string> enumList = new List<string>();

            if (property2enumType.ContainsKey(propertyId))
            {
                Type type = property2enumType[propertyId];

                for (int i = 0; i < Enum.GetValues(type).Length; i++)
                {
                    enumList.Add(Enum.GetValues(type).GetValue(i).ToString());

                }
            }
            else
            {
                string message = $"{baseLogString} GetEnumList => Failed to get enum list. Property ({propertyId} is not of enum type.";
                Logger.LogError(message);
                throw new Exception(message);
            }

            return enumList;
        }

        public List<string> GetEnumList(Type enumType)
        {
            List<string> enumList = new List<string>();

            try
            {
                for (int i = 0; i < Enum.GetValues(enumType).Length; i++)
                {
                    enumList.Add(Enum.GetValues(enumType).GetValue(i).ToString());
                }

                return enumList;
            }
            catch
            {
                string message = $"{baseLogString} GetEnumList => Failed to get enum list. Type ({enumType}) is not of enum type.";
                Logger.LogError(message);
                throw new Exception(message);
            }
        }

        public Type GetEnumTypeForPropertyId(ModelCode propertyId)
        {
            if (property2enumType.ContainsKey(propertyId))
            {
                return property2enumType[propertyId];
            }
            else
            {
                string message = $"{baseLogString} GetEnumTypeForPropertyId => Property ({propertyId}) is not of enum type.";
                Logger.LogError(message);
                throw new Exception(message);
            }
        }

        public Type GetEnumTypeForPropertyId(ModelCode propertyId, bool throwsException)
        {
            if (property2enumType.ContainsKey(propertyId))
            {
                return property2enumType[propertyId];
            }
            else if (throwsException)
            {
                string message = $"{baseLogString} GetEnumTypeForPropertyId => Property ({propertyId}) is not of enum type.";
                Logger.LogError(message);
                throw new Exception(message);
            }
            else
            {
                return null;
            }
        }

        public short GetEnumValueFromString(ModelCode propertyId, string value)
        {
            Type type = GetEnumTypeForPropertyId(propertyId);

            if (Enum.GetUnderlyingType(type) == typeof(short))
            {
                return (short)Enum.Parse(type, value);
            }
            else if (Enum.GetUnderlyingType(type) == typeof(uint))
            {
                return (short)((uint)Enum.Parse(type, value));
            }
            else if (Enum.GetUnderlyingType(type) == typeof(byte))
            {
                return (short)((byte)Enum.Parse(type, value));
            }
            else if (Enum.GetUnderlyingType(type) == typeof(sbyte))
            {
                return (short)((sbyte)Enum.Parse(type, value));
            }
            else
            {
                string message = $"{baseLogString} GetEnumValueFromString => Failed to get enum value from string ({value}). Invalid underlying type.";
                Logger.LogError(message);
                throw new Exception(message);
            }
        }

        public string GetStringFromEnum(ModelCode propertyId, short enumValue)
        {
            if (property2enumType.ContainsKey(propertyId))
            {
                string retVal = Enum.GetName(GetEnumTypeForPropertyId(propertyId), enumValue);
                if (retVal != null)
                {
                    return retVal;
                }
                else
                {
                    return enumValue.ToString();
                }
            }
            else
            {
                return enumValue.ToString();
            }
        }
    }
}

