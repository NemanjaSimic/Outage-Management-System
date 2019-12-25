﻿using Outage.SCADA.ModBus.Connection;
using Outage.SCADA.SCADA_Common;

namespace Outage.SCADA.ModBus.PointModels
{
    public class AnalogOutput : BasePointItem
    {
        private ushort value;
        //private string egu;

        public ushort Value
        {
            get
            {
                return value;
            }

            set
            {
                this.value = value;
            }
        }

        //public string Egu
        //{
        //    get
        //    {
        //        return egu;
        //    }

        //    set
        //    {
        //        egu = value;

        //    }
        //}

        public AnalogOutput(PointType type, ushort address, ushort defaultValue, FunctionExecutor commandExecutor) : base(type, address, commandExecutor, defaultValue)
        {
            Value = defaultValue;
        }

        public AnalogOutput(PointType type, ushort address, ushort defaultValue, FunctionExecutor commandExecutor, ushort minValue, ushort maxValue) : this(type, address, defaultValue, commandExecutor)
        {
            this.Min = minValue;
            this.Max = maxValue;
        }

        protected override void CommandExecutor_UpdatePointEvent(PointType type, ushort pointAddress, ushort newValue)
        {
            if (this.Type == type && this.Address == pointAddress)
            {
                Value = newValue;
                RawValue = newValue;
            }
        }
    }
}