using MediatR;
using OMS.Web.Common.Constants;

namespace OMS.Web.Services.Commands
{
    public abstract class SwitchCommandBase : IRequest
    {
        protected long _gid;
        protected SwitchCommandType _command;

        public long Gid
        {
            get { return _gid; }

            protected set { _gid = value; }
        }

        public SwitchCommandType Command
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
