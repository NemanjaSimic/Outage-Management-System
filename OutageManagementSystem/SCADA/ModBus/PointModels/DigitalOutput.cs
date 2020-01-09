using Outage.SCADA.ModBus.Connection;
using Outage.SCADA.SCADACommon;

namespace Outage.SCADA.ModBus.PointModels
{
    public class DigitalOutput : BasePointItem
    {
        private DState state;
        //private DState abnormalState = DState.OPENED;

        public DState State
        {
            get
            {
                return state;
            }

            set
            {
                state = value;
            }
        }

        //public DState AbnormalState
        //{
        //    get
        //    {
        //        return abnormalState;
        //    }

        //    set
        //    {
        //        abnormalState = value;

        //    }
        //}

        public DigitalOutput(PointType type, ushort address, ushort defaultValue, FunctionExecutor commandExecutor) : base(type, address, commandExecutor, defaultValue)
        {
            State = (DState)defaultValue;
        }

        public DigitalOutput(PointType type, ushort address, ushort defaultValue, FunctionExecutor commandExecutor, ushort minValue, ushort maxValue) : this(type, address, defaultValue, commandExecutor)
        {
            this.Min = minValue;
            this.Max = maxValue;
        }

        protected override void CommandExecutor_UpdatePointEvent(PointType type, ushort pointAddress, ushort newValue)
        {
            if (this.Type == type && this.Address == pointAddress)
            {
                State = (DState)newValue;
                RawValue = newValue;
            }
        }
    }
}