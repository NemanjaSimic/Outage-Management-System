using Outage.Common;

namespace Outage.SCADA.SCADACommon
{
    public interface ISCADAModelPointItem
    {
        long Gid { get; set; }
        ushort Address { get; set; }
        string Name { get; set; }
        PointType RegisterType { get; set; }
        AlarmType Alarm { get; set; }

        bool SetAlarms();
        ISCADAModelPointItem Clone();
    }

    public interface IAnalogSCADAModelPointItem : ISCADAModelPointItem
    {
        
        double CurrentEguValue { get; set; }
        double NormalValue { get; set; }
        double EGU_Min { get; set; }
        double EGU_Max { get; set; }
        float ScaleFactor { get; set; }
        float Deviation { get; set; }

        int CurrentRawValue { get; }
        int MinRawValue { get; }
        int MaxRawValue { get; }

        double HighLimit { get; set; }
        double LowLimit { get; set; }

        double RawToEguValueConversion(int rawValue);
        int EguToRawValueConversion(double eguValue);
    }

    public interface IDiscreteSCADAModelPointItem : ISCADAModelPointItem
    {
        ushort MinValue { get; set; }
        ushort MaxValue { get; set; }
        ushort NormalValue { get; set; }
        ushort CurrentValue { get; set; }
        ushort AbnormalValue { get; set; }
        
    }
}