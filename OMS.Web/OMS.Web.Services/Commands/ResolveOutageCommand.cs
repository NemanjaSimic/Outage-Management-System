namespace OMS.Web.Services.Commands
{
    public class ResolveOutageCommand : OutageLifecycleCommandBase
    {
        public ResolveOutageCommand(long outageId) : base(outageId) { }
    }
}
