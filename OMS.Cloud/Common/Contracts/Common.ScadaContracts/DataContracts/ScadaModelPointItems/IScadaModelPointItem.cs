using OMS.Common.Cloud;
using OMS.Common.SCADA;

namespace OMS.Common.ScadaContracts.DataContracts.ScadaModelPointItems
{
    //public interface IScadaModelPointItem
    //{
    //    long Gid { get; set; }
    //    ushort Address { get; set; }
    //    string Name { get; set; }
    //    PointType RegisterType { get; set; }
    //    AlarmType Alarm { get; }
    //    bool Initialized { get; set; }

    //    IScadaModelPointItem Clone();
    //}

    //public interface IAnalogPointItem : ScadaModelPointItem
    //{
    //    float CurrentEguValue { get; set; }
    //    float NormalValue { get; set; }
    //    float EGU_Min { get; set; }
    //    float EGU_Max { get; set; }
    //    float ScalingFactor { get; set; }
    //    float Deviation { get; set; }

    //    int CurrentRawValue { get; }
    //    int MinRawValue { get; }
    //    int MaxRawValue { get; }
    //    AnalogMeasurementType AnalogType { get; set; }

    //    float RawToEguValueConversion(int rawValue);
    //    int EguToRawValueConversion(float eguValue);
    //}

    //public interface IDiscretePointItem : IScadaModelPointItem
    //{
    //    ushort MinValue { get; set; }
    //    ushort MaxValue { get; set; }
    //    ushort NormalValue { get; set; }
    //    ushort CurrentValue { get; set; }
    //    ushort AbnormalValue { get; set; }
    //    DiscreteMeasurementType DiscreteType { get; set; }   
    //}
}