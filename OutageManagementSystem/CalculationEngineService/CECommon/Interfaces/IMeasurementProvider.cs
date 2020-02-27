using CECommon.Model;
using Outage.Common;
using Outage.Common.PubSub.SCADADataContract;
using System.Collections.Generic;

namespace CECommon.Interfaces
{
    public delegate void DiscreteMeasurementDelegate(List<long> signalsGid);
    public interface IMeasurementProvider
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
        void UpdateAnalogMeasurement(long measurementGid, float value, CommandOriginType commandOrigin);
        void UpdateAnalogMeasurement(Dictionary<long, AnalogModbusData> data); //TODO: AnalogModbusData koji stigne sa SCADA prepakovati u lokali model na CE
        bool UpdateDiscreteMeasurement(long measurementGid, int value, CommandOriginType commandOrigin);
        void UpdateDiscreteMeasurement(Dictionary<long, DiscreteModbusData> data); //TODO: DiscreteModbusData koji stigne sa SCADA prepakovati u lokali model na CE
        Dictionary<long, long> GetMeasurementToElementMap();
    }
}
