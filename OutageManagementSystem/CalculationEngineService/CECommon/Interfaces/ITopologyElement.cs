using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon.Interfaces
{
    public interface ITopologyElement
    {
        long Id { get; set; }
        string Description { get; set; }
        string Mrid { get; set; }
        string Name { get; set; }
        long FirstEnd { get; set; }
        List<long> SecondEnd { get; set; }
        string DmsType { get; set; }
        IMeasurement Measurement { get; set; }
        bool IsRemote { get; set; }
        bool IsActive { get; set; }
        float NominalVoltage { get; set; }

        string GetMeasurementType();
        float GetMeasurementValue();
    }
}
