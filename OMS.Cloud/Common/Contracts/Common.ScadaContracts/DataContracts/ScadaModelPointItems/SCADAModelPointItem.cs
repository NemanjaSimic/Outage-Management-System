using Outage.Common;
using OMS.Common.SCADA;
using System.Runtime.Serialization;

namespace OMS.Common.ScadaContracts.DataContracts.ScadaModelPointItems
{
    [DataContract]
    [KnownType(typeof(AnalogPointItem))]
    [KnownType(typeof(DiscretePointItem))]
    [KnownType(typeof(AlarmConfigData))]
    public abstract class ScadaModelPointItem : IScadaModelPointItem //, ICloneable
    {
        [DataMember]
        protected readonly IAlarmConfigData alarmConfigData;

        protected ScadaModelPointItem(IAlarmConfigData alarmConfigData)
        {
            this.alarmConfigData = alarmConfigData;
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
