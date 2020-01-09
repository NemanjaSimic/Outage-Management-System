namespace Outage.SCADA.SCADA_Common
{
    public interface IConfiguration
    {
        int TcpPort { get; set; }
        byte UnitAddress { get; set; }
    }
}