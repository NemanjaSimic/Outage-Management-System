namespace OMS.Common.ScadaContracts.DataContracts
{
    public interface ISetAlarmStrategy
    {
        bool SetAlarm(IScadaModelPointItem pointItem);
    }
}
