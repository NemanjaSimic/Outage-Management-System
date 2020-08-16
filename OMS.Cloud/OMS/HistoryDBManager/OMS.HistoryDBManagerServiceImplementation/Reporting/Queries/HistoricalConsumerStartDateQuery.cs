using Common.OMS.OutageDatabaseModel;
using OMS.HistoryDBManagerImplementation.Reporting.Queries.Specifications;
using System;
using System.Linq.Expressions;

namespace OMS.HistoryDBManagerImplementation.Reporting.Queries
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
