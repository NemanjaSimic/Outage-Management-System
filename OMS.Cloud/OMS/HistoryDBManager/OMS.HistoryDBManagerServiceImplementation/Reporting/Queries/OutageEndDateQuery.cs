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
    public class OutageEndDateQuery : OutageSpecification
    {
        private readonly DateTime _endDate;

        public OutageEndDateQuery(DateTime endDate)
            => _endDate = endDate;

        public override Expression<Func<OutageEntity, bool>> IsSatisfiedBy => x => x.ReportTime <= _endDate;
    }
}
