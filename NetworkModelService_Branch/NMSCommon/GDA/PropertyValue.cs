using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using System.ServiceModel;

namespace FTN.Common
{
    [Serializable]
    [DataContract]	
	public class PropertyValue
	{
        List<long> longValues = new List<long>();
        List<float> floatValues = new List<float>();
        List<string> stringValues = new List<string>();

        public PropertyValue()
        {
        }

        public PropertyValue(PropertyValue toCopy)
        {
            this.longValues = toCopy.longValues.GetRange(0, toCopy.longValues.Count);
            this.floatValues = toCopy.floatValues.GetRange(0, toCopy.floatValues.Count);
            this.stringValues = toCopy.stringValues.GetRange(0, toCopy.stringValues.Count);
        }

        public PropertyValue(long longValue)
        {
            this.LongValue = longValue;
        }

        public PropertyValue(float floatValue)
        {
            this.FloatValue = floatValue;
        }

        public PropertyValue(string stringValue)
        {
            this.StringValue = stringValue;
        }

        public PropertyValue(List<long> longValues)
        {
            this.LongValues = longValues;
        }

        public PropertyValue(List<float> floatValues)
        {
            this.FloatValues = floatValues;
        }

        public PropertyValue(List<string> stringValues)
        {
            this.StringValues = stringValues;
        }

		public long LongValue
		{
            get { return longValues.Count == 0 ? 0 : longValues.First(); }
            set { longValues.Clear(); longValues.Add(value); }
		}

		public float FloatValue
		{
            get { return floatValues.Count == 0 ? 0 : floatValues.First(); }
            set { floatValues.Clear(); floatValues.Add(value); }
		}

		public string StringValue
		{
            get { return stringValues.Count == 0 ? string.Empty : stringValues.First(); }
            set { stringValues.Clear(); stringValues.Add(value); }
		}
	
		[DataMember]
		public List<long> LongValues
		{
            get { return longValues; }
            set { longValues = value; }
		}

        [DataMember]
		public List<float> FloatValues
		{
            get { return floatValues; }
            set { floatValues = value; }
		}

        [DataMember]
		public List<string> StringValues
		{
            get { return stringValues; }
            set { stringValues = value; }
		}	


		/// <summary>
		/// Compares two PropertyValue objects on specific way - used for joins.
		/// </summary>
		/// <param name="first"></param>
		/// <param name="second"></param>
		/// <returns>TRUE if condition is fulfilled and FALSE in other case.</returns>
		public static bool operator ==(PropertyValue first, PropertyValue second)
		{
			if (Object.ReferenceEquals(first, null) && Object.ReferenceEquals(second, null))
			{
				return true;
			}
			else if ((Object.ReferenceEquals(first, null) && !Object.ReferenceEquals(second, null)) || (!Object.ReferenceEquals(first, null) && Object.ReferenceEquals(second, null)))
			{
				return false;
			}
			else
			{
				if (!CompareHelper.CompareLists(first.longValues, second.longValues))
                {
                    return false;
				}
                else if (!CompareHelper.CompareLists(first.stringValues, second.stringValues))
                {
                    return false;
                }
                else if (!CompareHelper.CompareLists(first.floatValues, second.floatValues))
                {
                    return false;
                }

                return true;
			}
		}

		public static bool operator !=(PropertyValue first, PropertyValue second)
		{
			return !(first == second);
		}

		public override bool Equals(object obj)
		{
			return obj is PropertyValue && this == (PropertyValue)obj;
		}

		public override int GetHashCode()
		{
			int hashCode = longValues.GetHashCode() + floatValues.GetHashCode() + stringValues.GetHashCode();	
			return hashCode;
		}
	}
}
