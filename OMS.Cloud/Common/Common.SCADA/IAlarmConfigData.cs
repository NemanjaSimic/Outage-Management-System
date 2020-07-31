namespace OMS.Common.SCADA
{
    public interface IAlarmConfigData
    {
        float HighCurrentLimit { get; }
        float HighPowerLimit { get; }
        float HighVolageLimit { get; }
        float LowCurrentLimit { get; }
        float LowPowerLimit { get; }
        float LowVoltageLimit { get; }
    }
}