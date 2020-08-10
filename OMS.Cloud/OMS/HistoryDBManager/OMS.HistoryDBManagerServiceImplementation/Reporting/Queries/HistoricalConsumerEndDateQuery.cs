using Common.OMS.OutageDatabaseModel;
using OMS.HistoryDBManagerServiceImplementation.Reporting.Queries.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OMS.HistoryDBManagerServiceImplementation.Reporting.Queries
{
    public class HistoricalConsumerEndDateQuery : HistoricalConsumerSpecification
    {
        private readonly DateTime _date;

        public HistoricalConsumerEndDateQuery(DateTime date)
            => _date = date;

        public override Expression<Func<ConsumerHistorical, bool>> IsSatisfiedBy => x => x.OperationTime <= _date;
    }
}
