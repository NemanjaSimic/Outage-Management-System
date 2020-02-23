namespace OMS.Web.Services.Commands
{
    public class IsolateOutageCommand : OutageLifecycleCommandBase
    {
        public IsolateOutageCommand(long outageId) : base(outageId) { }
    }
}
