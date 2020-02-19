using CECommon.Model;
using Outage.Common.PubSub.SCADADataContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon.Interfaces
{
    public delegate void DiscreteMeasurementDelegate(List<long> signalsGid);
    public interface ICacheProvider
    {
        DiscreteMeasurementDelegate DiscreteMeasurementDelegate { get; set; }
        void AddAnalogMeasurement(AnalogMeasurement analogMeasurement);
        void AddDiscreteMeasurement(DiscreteMeasurement discreteMeasurement);
        float GetAnalogValue(long measurementGid);
        bool GetDiscreteValue(long measurementGid);
        long GetElementGidForMeasurement(long measurementGid);
        List<long> GetMeasurementsForElement(long elementGid);
        Dictionary<long, List<long>> GetElementToMeasurementMap();
        bool TryGetAnalogMeasurement(long measurementGid, out AnalogMeasurement measurement);
        bool TryGetDiscreteMeasurement(long measurementGid, out DiscreteMeasurement measurement);
        void UpdateAnalogMeasurement(long measurementGid, float value);
        void UpdateAnalogMeasurement(Dictionary<long, AnalogModbusData> data);
        void UpdateDiscreteMeasurement(long measurementGid, ushort value);
        void UpdateDiscreteMeasurement(Dictionary<long, DiscreteModbusData> data);
        Dictionary<long, long> GetMeasurementToElementMap();
    }
}
