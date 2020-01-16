using CECommon.Model;

namespace CECommon.Interfaces
{
    public interface ITopologyBuilder
    {
        TopologyModel CreateGraphTopology(long firstElementGid, TransactionFlag flag);
    }
}
