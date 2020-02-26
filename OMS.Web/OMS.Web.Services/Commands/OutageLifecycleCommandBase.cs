namespace OMS.Web.Services.Commands
{
    using MediatR;
    
    public abstract class OutageLifecycleCommandBase : IRequest
    {
        protected long _outageId;

        public long OutageId
        {
            get => _outageId;
            set => _outageId = value;
        }

        public OutageLifecycleCommandBase(long outageId) => _outageId = outageId;
    }
}
