namespace Common.Web.Services.Commands
{
    public class SendOutageLocationIsolationCrewCommand : OutageLifecycleCommandBase
    {
        public SendOutageLocationIsolationCrewCommand(long outageId) : base(outageId) { }
    }
}
