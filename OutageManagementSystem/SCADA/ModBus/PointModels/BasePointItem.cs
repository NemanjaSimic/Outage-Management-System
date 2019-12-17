using SCADA_Common;
using ModBus.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.PointModels
{
    public class BasePointItem
    {

        private ushort rawValue;
        private ushort min;
        private ushort max;
        private PointType type;
        private ushort address;

        public BasePointItem(PointType type, ushort address, FunctionExecutor commandExecutor, ushort defaultValue)
        {
            this.type = type;
            this.address = address;
            this.rawValue = defaultValue;
            commandExecutor.UpdatePointEvent += CommandExecutor_UpdatePointEvent;

        }

        public BasePointItem()
        {
        }

        protected virtual void CommandExecutor_UpdatePointEvent(PointType type, ushort pointAddres, ushort newValue)
        {
            // Intentionally left blank
        }

        /// <summary>
		/// Address of point on MdbSim Simulator
		/// </summary>
		public ushort Address
        {
            get
            {
                return address;
            }

            set
            {
                address = value;
                
            }
        }

        public PointType Type
        {
            get
            {
                return type;
            }

            set
            {
                type = value;
               
            }
        }

        public ushort Min
        {
            get
            {
                return min;
            }

            set
            {
                min = value;
                
            }
        }

        public ushort Max
        {
            get
            {
                return max;
            }

            set
            {
                max = value;
                
            }
        }

        public ushort RawValue
        {
            get
            {
                return rawValue;
            }

            set
            {
                rawValue = value;

            }
        }
    }
}
