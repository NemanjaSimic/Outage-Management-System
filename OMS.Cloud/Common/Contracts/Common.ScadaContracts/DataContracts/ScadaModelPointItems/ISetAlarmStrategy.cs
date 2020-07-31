namespace OMS.Common.ScadaContracts.DataContracts.ScadaModelPointItems
{
    public interface ISetAlarmStrategy
    {
        bool SetAlarm(IScadaModelPointItem pointItem);
    }
}
