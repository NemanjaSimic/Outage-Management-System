using CECommon.Model;

namespace CECommon.Interfaces
{
    public interface ITopologyBuilder
    {
        ITopology CreateGraphTopology(long firstElementGid);
    }
}
