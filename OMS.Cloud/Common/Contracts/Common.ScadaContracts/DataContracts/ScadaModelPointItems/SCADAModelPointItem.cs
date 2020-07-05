using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
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

        [DataMember]
        protected string baseLogString;

        #region Private Properties
        private ICloudLogger logger;
        protected ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
        #endregion Private Properties

        #region Public Properties
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
        #endregion Public Properties

        protected ScadaModelPointItem(IAlarmConfigData alarmConfigData)
        {
            this.alarmConfigData = alarmConfigData;
        }

        public abstract bool SetAlarms();

        #region IClonable

        public abstract IScadaModelPointItem Clone();


        #endregion IClonable
    }
}
