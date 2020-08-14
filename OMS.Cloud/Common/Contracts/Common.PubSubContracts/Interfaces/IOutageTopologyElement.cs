using System.Collections.Generic;

namespace OMS.Common.PubSubContracts.Interfaces
{
    public interface IOutageTopologyElement
    {
        long Id { get; set; }
        long FirstEnd { get; set; }
        List<long> SecondEnd { get; set; }
        string DmsType { get; set; }
        bool IsRemote { get; set; }
        bool IsActive { get; set; }
        ushort DistanceFromSource { get; set; }
        bool NoReclosing { get; set; }
        bool IsOpen { get; set; }
    }
}
