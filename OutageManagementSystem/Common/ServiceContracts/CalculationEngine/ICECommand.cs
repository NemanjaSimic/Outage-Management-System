using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

//TODO: sta smo planirali sa ovim?
namespace Outage.Common.ServiceContracts.CalculationEngine
{
    [ServiceContract]
    interface ICECommand : IService
    {
        [OperationContract]
        bool SendAnalogCommand(long gid, float commandingValue);

        [OperationContract]
        bool SendDiscreteCommand(long gid, ushort commandingValue);
    }
}
