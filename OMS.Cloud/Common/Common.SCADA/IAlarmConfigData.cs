namespace OMS.Common.SCADA
{
    public interface IAlarmConfigData
    {
        float HighFeederCurrentLimit { get; }
        float LowFeederCurrentLimit { get; }

        float HighCurrentLimit { get; }
        float LowCurrentLimit { get; }

        float HighPowerLimit { get; }
        float LowPowerLimit { get; }

        float HighVolageLimit { get; }
        float LowVoltageLimit { get; }
    }
}