using System.Fabric;

namespace OMS.Common.Cloud.Logger
{
    public interface IServiceEventTracing
    {
        void UniversalServiceMessage(ServiceContext serviceContext, string message);
    }
}
