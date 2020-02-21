namespace OMS.Web.Services.Commands
{
    using MediatR;
    
    public abstract class OutageLifecycleCommandBase : IRequest
    {
        protected long _id;

        public long Id
        {
            get => _id;
            set => _id = value;
        }

        public OutageLifecycleCommandBase(long id) => _id = id;
    }
}
