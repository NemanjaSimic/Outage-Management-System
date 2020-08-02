using Common.OMS.OutageDatabaseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OMS.ReportingServiceImplementation.Queries
{
    public class OutageStartDateQuery : OutageSpecification
    {
        private readonly DateTime _startDate;

        public OutageStartDateQuery(DateTime startDate)
            => _startDate = startDate;

        public override Expression<Func<OutageEntity, bool>> IsSatisfiedBy => x => x.ReportTime >= _startDate;
    }
}
