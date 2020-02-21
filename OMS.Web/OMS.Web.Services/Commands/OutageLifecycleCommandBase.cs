namespace OMS.Web.Services.Commands
{
    using MediatR;
    
    public abstract class OutageLifecycleCommandBase : IRequest
    {
<<<<<<< HEAD
        protected long _outageId;

        public long OutageId
        {
            get => _outageId;
            set => _outageId = value;
        }

        public OutageLifecycleCommandBase(long outageId) => _outageId = outageId;
=======
        protected long _id;

        public long Id
        {
            get => _id;
            set => _id = value;
        }

        public OutageLifecycleCommandBase(long id) => _id = id;
>>>>>>> Outage Lifecycle services
    }
}
