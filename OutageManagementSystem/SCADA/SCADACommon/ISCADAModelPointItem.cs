namespace Outage.SCADA.SCADACommon
{
    public interface ISCADAModelPointItem
    {
        long Gid { get; set; }
        ushort Address { get; set; }
        string Name { get; set; }
        PointType RegistarType { get; set; }
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

        ISCADAModelPointItem Clone();
    }
}