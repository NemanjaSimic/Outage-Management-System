using System.Collections.Generic;

namespace CECommon.Interfaces
{
    public interface ITopologyElement : IGraphElement
    {
        long Id { get; set; }
        string Description { get; set; }
        string Mrid { get; set; }
        string Name { get; set; }
        ITopologyElement FirstEnd { get; set; }
        List<ITopologyElement> SecondEnd { get; set; }
        string DmsType { get; set; }
        bool IsRemote { get; set; }
        bool IsActive { get; set; }
        float NominalVoltage { get; set; }
        List<long> Measurements { get; set; }
        bool NoReclosing { get; set; }
    }
}
