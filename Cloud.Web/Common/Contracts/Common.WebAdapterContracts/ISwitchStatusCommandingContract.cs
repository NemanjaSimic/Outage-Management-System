using System;
using System.ServiceModel;

namespace Common.Contracts.WebAdapterContracts
{
    [ServiceContract]
    public interface ISwitchStatusCommandingContract
    {
        //TERMINOLOGY: using 'gid' because UI can send gid of either element (for noSCADA commanding) or measurement (for SCADA commanding)

        [OperationContract]
        [Obsolete("Use SendOpenCommand and SendCloseCommand methods instead")]
        void SendSwitchCommand(long gid, int value);

        [OperationContract]
        void SendOpenCommand(long gid);

        [OperationContract]
        void SendCloseCommand(long gid);
    }
}
