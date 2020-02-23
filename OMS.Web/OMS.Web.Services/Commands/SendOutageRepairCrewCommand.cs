namespace OMS.Web.Services.Commands
{
    public class SendOutageRepairCrewCommand : OutageLifecycleCommandBase
    {
        public SendOutageRepairCrewCommand(long outageId) : base(outageId) { }
    }
}
