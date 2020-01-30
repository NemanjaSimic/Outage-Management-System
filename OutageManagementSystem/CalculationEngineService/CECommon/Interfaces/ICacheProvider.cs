using CECommon.Model;
using Outage.Common.PubSub.SCADADataContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon.Interfaces
{
    public delegate void DiscreteMeasurementDelegate(long signalGid);
    public interface ICacheProvider
    {
        DiscreteMeasurementDelegate DiscreteMeasurementDelegate { get; set; }
        void AddAnalogMeasurement(AnalogMeasurement analogMeasurement);
        void AddDiscreteMeasurement(DiscreteMeasurement discreteMeasurement);
        float GetAnalogValue(long measurementGid);
        bool GetDiscreteValue(long measurementGid);
        void UpdateAnalogMeasurement(long measurementGid, float value);
        void UpdateAnalogMeasurement(Dictionary<long, AnalogModbusData> data);
        void UpdateDiscreteMeasurement(long measurementGid, ushort value);
        void UpdateDiscreteMeasurement(Dictionary<long, DiscreteModbusData> data);
    }
}
