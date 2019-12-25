namespace Outage.SCADA.SCADA_Common
{
    public interface IResponse
    {
        long GID { get; set; }
        AlarmType Alarm { get; set; }
        object Value { get; set; }
    }
}