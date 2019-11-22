using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTN.Common
{
	public class EnumDescs
	{
		private Dictionary<ModelCode, Type> property2enumType = new Dictionary<ModelCode, Type>();

		public EnumDescs()
        { 
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
				throw new Exception(string.Format("Failed to get enum list. Property ({0}) is not of enum type.", propertyId));
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
				throw new Exception(string.Format("Failed to get enum list. Type ({0}) is not of enum type.", enumType));
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
				throw new Exception(string.Format("Property ({0}) is not of enum type.", propertyId));
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
				throw new Exception(string.Format("Property ({0}) is not of enum type.", propertyId));
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
				throw new Exception(string.Format("Failed to get enum value from string ({0}). Invalid underlying type.", value));
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
