using Outage.Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.SCADA.SCADAService
{
    public class SCADAModel
    {
        //TODO: stanje "modela" -> npr string putanje ka dokumentu

        public bool Notify(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            throw new NotImplementedException();
        }

        public bool Prepare()
        {
            throw new NotImplementedException();
        }

        public void Commit()
        {
            throw new NotImplementedException();
        }

        public void Rollback()
        {
            throw new NotImplementedException();
        }
    }
}
