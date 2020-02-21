namespace OMS.Web.Services.Commands
{
    using MediatR;
    
    public abstract class OutageLifecycleCommandBase : IRequest
    {
        protected long _gid;

        public long Gid
        {
            get => _gid;
            set => _gid = value;
        }

        public OutageLifecycleCommandBase(long gid) => _gid = gid;
    }
}
