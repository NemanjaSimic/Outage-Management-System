using Outage.Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.ServiceContracts
{
    public interface ITransactionActorContract
    {
        bool EnlistUpdateDelta(Delta delta);
        void Prepare();
        bool Commit();
        bool Rollback();
    }
}
