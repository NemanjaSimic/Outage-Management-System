using Common.OMS.OutageDatabaseModel;
using OMS.Common.Cloud;
using OMS.HistoryDBManagerServiceImplementation.Reporting.Queries.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OMS.HistoryDBManagerServiceImplementation.Reporting.Queries
{
    public class OutageLifecycleQuery : OutageSpecification
    {
        private readonly OutageState _state;

        public OutageLifecycleQuery(OutageState state)
            => _state = state;

        public override Expression<Func<OutageEntity, bool>> IsSatisfiedBy => x => x.OutageState == _state;
    }
}
