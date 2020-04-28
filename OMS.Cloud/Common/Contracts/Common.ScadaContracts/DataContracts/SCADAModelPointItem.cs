using Outage.Common;
using OMS.Common.SCADA;
using System.Runtime.Serialization;
using Common.SCADA;

namespace OMS.Common.ScadaContracts.DataContracts
{
    [DataContract]
    public abstract class ScadaModelPointItem : IScadaModelPointItem //, ICloneable
    {
        protected readonly ISetAlarmStrategy alarmStrategy;

        protected ScadaModelPointItem(ISetAlarmStrategy alarmStrategy)
        {
            this.alarmStrategy = alarmStrategy;
        }

        [DataMember]
        public long Gid { get; set; }
        [DataMember]
        public ushort Address { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public PointType RegisterType { get; set; }
        [DataMember]
        public AlarmType Alarm { get; set; }
        [DataMember]
        public bool Initialized { get; set; }

        public abstract bool SetAlarms();

        #region IClonable

        public abstract IScadaModelPointItem Clone();


        #endregion IClonable

    }
}
