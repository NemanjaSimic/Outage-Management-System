namespace Common.Web.Services.Commands
{
    public class ValidateResolveConditionsCommand : OutageLifecycleCommandBase
    {
        public ValidateResolveConditionsCommand(long outageId) : base(outageId) { }
    }
}
