using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using OMS.HistoryDBManagerImplementation.Reporting.Queries.Specifications;
using System;
using System.Linq.Expressions;

namespace OMS.HistoryDBManagerImplementation.Reporting.Queries
{
    public class HistoricalConsumerEndDateQuery : HistoricalConsumerSpecification
    {
        private readonly DateTime _date;

        public HistoricalConsumerEndDateQuery(DateTime date)
            => _date = date;

        public override Expression<Func<ConsumerHistorical, bool>> IsSatisfiedBy => x => x.OperationTime <= _date;
    }
}
