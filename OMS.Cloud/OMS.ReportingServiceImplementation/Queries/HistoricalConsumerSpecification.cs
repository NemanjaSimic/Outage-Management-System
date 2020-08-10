using Common.OMS.OutageDatabaseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OMS.ReportingServiceImplementation.Queries
{
    public abstract class HistoricalConsumerSpecification : Specification<ConsumerHistorical>
    {
        public abstract override Expression<Func<ConsumerHistorical, bool>> IsSatisfiedBy { get; }
    }
}
