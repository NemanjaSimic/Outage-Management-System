using OMS.Common.PubSubContracts.DataContracts.SCADA;
using System.Collections.Generic;

namespace Common.CeContracts
{ 
    public delegate void DiscreteMeasurementDelegate();
    public interface IMeasurementProvider
    {
        DiscreteMeasurementDelegate DiscreteMeasurementDelegate { get; set; }
        void AddAnalogMeasurement(IAnalogMeasurement analogMeasurement);
        void AddDiscreteMeasurement(IDiscreteMeasurement discreteMeasurement);
        void AddMeasurementElementPair(long measurementId, long elementId);
        float GetAnalogValue(long measurementGid);
        bool GetDiscreteValue(long measurementGid);
        long GetElementGidForMeasurement(long measurementGid);
        List<long> GetMeasurementsOfElement(long elementGid);
        Dictionary<long, List<long>> GetElementToMeasurementMap();
        bool TryGetAnalogMeasurement(long measurementGid, out IAnalogMeasurement measurement);
        bool TryGetDiscreteMeasurement(long measurementGid, out IDiscreteMeasurement measurement);
        void UpdateAnalogMeasurement(Dictionary<long, AnalogModbusData> data); //TODO: AnalogModbusData koji stigne sa SCADA prepakovati u lokali model na CE
        void UpdateDiscreteMeasurement(Dictionary<long, DiscreteModbusData> data); //TODO: DiscreteModbusData koji stigne sa SCADA prepakovati u lokali model na CE
        Dictionary<long, long> GetMeasurementToElementMap();
        bool PrepareForTransaction();
        void CommitTransaction();
        void RollbackTransaction();
    }
}
