using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
