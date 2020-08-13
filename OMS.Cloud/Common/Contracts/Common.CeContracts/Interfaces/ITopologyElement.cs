using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Common.CeContracts
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
        Dictionary<long, string> Measurements { get; set; }
        bool NoReclosing { get; set; }
        ITopologyElement Feeder { get; set; }
    }

    [KnownType(typeof(EnergyConsumer))]
    [KnownType(typeof(Feeder))]
    [KnownType(typeof(Field))]
    [KnownType(typeof(Recloser))]
    [KnownType(typeof(SynchronousMachine))]
    [KnownType(typeof(TopologyElement))]
    public abstract class AbstractTopologyElement : ITopologyElement
    {
        public abstract long Id { get; set; }
        public abstract string Description { get; set; }
        public abstract string Mrid { get; set; }
        public abstract string Name { get; set; }
        public abstract ITopologyElement FirstEnd { get; set; }
        public abstract List<ITopologyElement> SecondEnd { get; set; }
        public abstract string DmsType { get; set; }
        public abstract bool IsRemote { get; set; }
        public abstract bool IsActive { get; set; }
        public abstract float NominalVoltage { get; set; }
        public abstract Dictionary<long, string> Measurements { get; set; }
        public abstract bool NoReclosing { get; set; }
        public abstract ITopologyElement Feeder { get; set; }
    }
}


