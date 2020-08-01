using CECommon.Model;
using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.Cloud;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.CeContracts
{
    [ServiceContract]
	public interface IMeasurementProviderContract : IService
	{
        [OperationContract]
        Task AddAnalogMeasurement(AnalogMeasurement analogMeasurement);
        [OperationContract]
        Task AddDiscreteMeasurement(DiscreteMeasurement discreteMeasurement);
        [OperationContract]
        Task AddMeasurementElementPair(long measurementId, long elementId);
        [OperationContract]
        Task<float> GetAnalogValue(long measurementGid);
        [OperationContract]
        Task<bool> GetDiscreteValue(long measurementGid);
        [OperationContract]
        Task<long> GetElementGidForMeasurement(long measurementGid);
        [OperationContract]
        Task<List<long>> GetMeasurementsOfElement(long elementGid);
        [OperationContract]
        Task<Dictionary<long, List<long>>> GetElementToMeasurementMap();
        [OperationContract]
        Task<AnalogMeasurement> GetAnalogMeasurement(long measurementGid);
        [OperationContract]
        Task<DiscreteMeasurement> GetDiscreteMeasurement(long measurementGid);
        [OperationContract]
        Task UpdateAnalogMeasurement(Dictionary<long, AnalogModbusData> data); //TODO: AnalogModbusData koji stigne sa SCADA prepakovati u lokali model na CE
        [OperationContract]
        Task UpdateDiscreteMeasurement(Dictionary<long, DiscreteModbusData> data); //TODO: DiscreteModbusData koji stigne sa SCADA prepakovati u lokali model na CE
        [OperationContract]
        Task<Dictionary<long, long>> GetMeasurementToElementMap();
        [OperationContract]
        Task<bool> PrepareForTransaction();
        [OperationContract]
        Task CommitTransaction();
        [OperationContract]
        Task RollbackTransaction();
        [OperationContract]
        Task SendAnalogCommand(long measurementGid, float commandingValue, CommandOriginType commandOrigin);
        [OperationContract]
        Task SendDiscreteCommand(long measurementGid, int value, CommandOriginType commandOrigin);
    }
}
