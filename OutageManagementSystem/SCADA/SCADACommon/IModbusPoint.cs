namespace Outage.SCADA.SCADACommon
{
    public interface IModbusPoint
    {
        string Name { get; set; }
        long Gid { get; set; }
        PointType RegistarType { get; set; }
        ushort Address { get; set; }
        float MinValue { get; set; }
        float MaxValue { get; set; }
        float DefaultValue { get; set; }
        float CurrentValue { get; set; }
        double ScaleFactor { get; set; }
        double Deviation { get; set; }
        double EGU_Min { get; set; }
        double EGU_Max { get; set; }
        ushort AbnormalValue { get; set; }
        double HighLimit { get; set; }

        double LowLimit { get; set; }
    }
}