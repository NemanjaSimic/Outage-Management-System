namespace CECommon.Interfaces
{
    public interface IMeasurement : IGraphElement
    {
        long Id { get; set; }
        string Address { get; set; }
        bool isInput { get; set; }
        long ElementId { get; set; }

        string GetMeasurementType();
        float GetCurrentVaule();
    }
}
