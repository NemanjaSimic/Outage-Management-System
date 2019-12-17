using SCADA_Common;
using ModBus.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.PointModels
{
    public class DigitalInput : BasePointItem
    {
        private DState state;
        //private DState abnormalState = DState.OPENED;

        public DigitalInput(PointType type, ushort address, ushort defaultValue, FunctionExecutor commandExecutor) : base(type, address, commandExecutor, defaultValue)
        {
            State = (DState)defaultValue;
        }

        public DigitalInput(PointType type, ushort address, ushort defaultValue, FunctionExecutor commandExecutor, ushort minValue, ushort maxValue) : this(type, address, defaultValue, commandExecutor)
        {
            this.Min = minValue;
            this.Max = maxValue;
        }

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
