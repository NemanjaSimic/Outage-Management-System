using Outage.Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.ServiceContracts
{
    public interface ITransactionCoordinatorContract
    {
        void StartDistributedUpdate(Delta delta, string actorName);
        void FinishDistributedUpdate(string actorName, bool success);
    }
}
