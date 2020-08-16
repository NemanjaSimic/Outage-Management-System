using Common.OMS.OutageDatabaseModel;
using OMS.HistoryDBManagerImplementation.Reporting.Queries.Specifications;
using System;
using System.Linq.Expressions;

namespace OMS.HistoryDBManagerImplementation.Reporting.Queries
{
    public class OutageEndDateQuery : OutageSpecification
    {
        private readonly DateTime _endDate;

        public OutageEndDateQuery(DateTime endDate)
            => _endDate = endDate;

        public override Expression<Func<OutageEntity, bool>> IsSatisfiedBy => x => x.ReportTime <= _endDate;
    }
}
