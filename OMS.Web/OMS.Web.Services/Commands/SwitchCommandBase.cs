using MediatR;
using OMS.Web.Common.Constants;

namespace OMS.Web.Services.Commands
{
    public abstract class SwitchCommandBase : IRequest
    {
        protected long _gid;
        protected SwitchCommand _command;

        public long Gid
        {
            get { return _gid; }

            protected set { _gid = value; }
        }

        public SwitchCommand Command
        {
            get { return _command; }
            protected set { _command = value; }
        }

        public SwitchCommandBase(long gid)
        {
            _gid = gid;
        }
    }
}
