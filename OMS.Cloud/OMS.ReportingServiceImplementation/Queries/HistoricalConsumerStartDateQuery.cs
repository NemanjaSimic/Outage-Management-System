using Common.OMS.OutageDatabaseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OMS.ReportingServiceImplementation.Queries
{
    public class HistoricalConsumerStartDateQuery : HistoricalConsumerSpecification
    {
        private readonly DateTime _date;

        public HistoricalConsumerStartDateQuery(DateTime date)
            => _date = date;

        public override Expression<Func<ConsumerHistorical, bool>> IsSatisfiedBy =>
            x => x.OperationTime >= _date;

    }
}
