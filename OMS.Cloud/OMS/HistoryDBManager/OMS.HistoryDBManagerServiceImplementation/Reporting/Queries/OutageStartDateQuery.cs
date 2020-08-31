using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using OMS.HistoryDBManagerImplementation.Reporting.Queries.Specifications;
using System;
using System.Linq.Expressions;

namespace OMS.HistoryDBManagerImplementation.Reporting.Queries
{
    public class OutageStartDateQuery : OutageSpecification
    {
        private readonly DateTime _startDate;

        public OutageStartDateQuery(DateTime startDate)
            => _startDate = startDate;

        public override Expression<Func<OutageEntity, bool>> IsSatisfiedBy => x => x.ReportTime >= _startDate;
    }
}
