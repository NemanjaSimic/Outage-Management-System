using Common.OMS.OutageDatabaseModel;
using System;
using System.Linq.Expressions;

namespace OMS.HistoryDBManagerImplementation.Reporting.Queries.Specifications
{
    public abstract class HistoricalConsumerSpecification : Specification<ConsumerHistorical>
    {
        public abstract override Expression<Func<ConsumerHistorical, bool>> IsSatisfiedBy { get; }
    }
}
