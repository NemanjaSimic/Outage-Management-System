using Outage.Common;
using Outage.Common.GDA;
using OMS.Cloud.SCADA.Common;
using System;
using System.Collections.Generic;

namespace OMS.Cloud.SCADA.Data.Repository
{
    public abstract class SCADAModelPointItem : ISCADAModelPointItem //, ICloneable
    {
        private ILogger logger;

        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        public long Gid { get; set; }
        public ushort Address { get; set; }
        public string Name { get; set; }
        public PointType RegisterType { get; set; }
        public AlarmType Alarm { get; set; }
        public bool Initialized { get; protected set; }

        protected SCADAModelPointItem(List<Property> props, ModelCode type)
        {
            Alarm = AlarmType.NO_ALARM;

            foreach (var item in props)
            {
                switch (item.Id)
                {
                    case ModelCode.IDOBJ_GID:
                        Gid = item.AsLong();
                        break;

                    case ModelCode.IDOBJ_NAME:
                        Name = item.AsString();
                        break;

                    case ModelCode.MEASUREMENT_ADDRESS:
                        if (ushort.TryParse(item.AsString(), out ushort address))
                        {
                            Address = address;
                        }
                        else
                        {
                            string message = "SCADAModelPointItem constructor => Address is either not defined or is invalid.";
                            Logger.LogError(message);
                            throw new ArgumentException(message);
                        }
                        break;

                    case ModelCode.MEASUREMENT_ISINPUT:
                        if (type == ModelCode.ANALOG)
                        {
                            RegisterType = (item.AsBool() == true) ? PointType.ANALOG_INPUT : PointType.ANALOG_OUTPUT;
                        }
                        else if (type == ModelCode.DISCRETE)
                        {
                            RegisterType = (item.AsBool() == true) ? PointType.DIGITAL_INPUT : PointType.DIGITAL_OUTPUT;
                        }
                        else
                        {
                            string message = "SCADAModelPointItem constructor => ModelCode type is neither ANALOG nor DISCRETE.";
                            Logger.LogError(message);
                            throw new ArgumentException(message);
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        protected abstract bool SetAlarms();

        #region IClonable

        public abstract ISCADAModelPointItem Clone();
        

        #endregion IClonable

    }
}