namespace Outage.Common.OutageService.Interface
{
    public interface IOutageTopologyModel
    {
        long FirstNode { get; set; }
        void AddElement(IOutageTopologyElement newElement);
        bool GetElementByGid(long gid, out IOutageTopologyElement topologyElement);
        bool GetElementByGidFirstEnd(long gid, out IOutageTopologyElement topologyElement);
    }
}
