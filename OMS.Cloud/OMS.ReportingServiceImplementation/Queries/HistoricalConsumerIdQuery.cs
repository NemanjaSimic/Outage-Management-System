using Common.OMS.OutageDatabaseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OMS.ReportingServiceImplementation.Queries
{
    public class HistoricalConsumerIdQuery : HistoricalConsumerSpecification
    {
        private readonly long _id;

        public HistoricalConsumerIdQuery(long id)
            => _id = id;

        public override Expression<Func<ConsumerHistorical, bool>> IsSatisfiedBy => x => x.ConsumerId == _id;
    }
}
