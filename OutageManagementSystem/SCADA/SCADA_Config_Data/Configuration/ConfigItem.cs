using Outage.SCADA.SCADA_Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.SCADA.SCADA_Config_Data.Configuration
{
    public class ConfigItem : IConfigItem
    {

        public PointType RegistarType { get; set; }
        public ushort Address { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; }
        public float DefaultValue { get; set; }
        public double ScaleFactor { get; set; }
        public double Deviation { get; set; }
        public double EGU_Min { get; set; }
        public double EGU_Max { get; set; }

        public ushort AbnormalValue { get; set; }

        public double HighLimit { get; set; }
        public double LowLimit { get; set; }
        public long Gid { get; set; }
        public string Name { get; set; }
        public float CurrentValue { get; set; }

        private PointType GetRegistryType(string registryTypeName)
        {
            PointType registryType;
            switch (registryTypeName)
            {
                case "DO_REG":
                    registryType = PointType.DIGITAL_OUTPUT;
                    break;

                case "DI_REG":
                    registryType = PointType.DIGITAL_INPUT;
                    break;

                case "IN_REG":
                    registryType = PointType.ANALOG_INPUT;
                    break;

                case "HR_INT":
                    registryType = PointType.ANALOG_OUTPUT;
                    break;

                default:
                    registryType = PointType.HR_LONG;
                    break;
            }
            return registryType;
        }

    }
}
