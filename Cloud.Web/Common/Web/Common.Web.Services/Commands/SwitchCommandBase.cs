using Common.Web.UI.Models.BindingModels;
using MediatR;

namespace Common.Web.Services.Commands
{
    public abstract class SwitchCommandBase : IRequest
    {
        protected long _gid;
        protected SwitchCommandType _command;

        public long Gid
        {
            get => _gid;
            protected set => _gid = value;
        }

        public SwitchCommandType Command
        {
            get => _command;
            protected set => _command = value;
        }

        public SwitchCommandBase(long gid)
           => _gid = gid;
    }
}
