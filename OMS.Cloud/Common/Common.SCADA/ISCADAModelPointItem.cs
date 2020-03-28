using Outage.Common;

namespace OMS.Common.SCADA
{
    public interface ISCADAModelPointItem
    {
        long Gid { get; set; }
        ushort Address { get; set; }
        string Name { get; set; }
        PointType RegisterType { get; set; }
        AlarmType Alarm { get; set; }
        bool Initialized { get; }

        ISCADAModelPointItem Clone();
    }

    public interface IAnalogSCADAModelPointItem : ISCADAModelPointItem
    {
        float CurrentEguValue { get; set; }
        float NormalValue { get; set; }
        float EGU_Min { get; set; }
        float EGU_Max { get; set; }
        float ScalingFactor { get; set; }
        float Deviation { get; set; }

        int CurrentRawValue { get; }
        int MinRawValue { get; }
        int MaxRawValue { get; }
        AnalogMeasurementType AnalogType { get; }

        float RawToEguValueConversion(int rawValue);
        int EguToRawValueConversion(float eguValue);
    }

    public interface IDiscreteSCADAModelPointItem : ISCADAModelPointItem
    {
        ushort MinValue { get; set; }
        ushort MaxValue { get; set; }
        ushort NormalValue { get; set; }
        ushort CurrentValue { get; set; }
        ushort AbnormalValue { get; set; }
        DiscreteMeasurementType DiscreteType { get; set; }   
    }
}