using Common.OMS.OutageDatabaseModel;
using OMS.Common.Cloud;
using OMS.HistoryDBManagerImplementation.Reporting.Queries.Specifications;
using System;
using System.Linq.Expressions;

namespace OMS.HistoryDBManagerImplementation.Reporting.Queries
{
    public class OutageLifecycleQuery : OutageSpecification
    {
        private readonly OutageState _state;

        public OutageLifecycleQuery(OutageState state)
            => _state = state;

        public override Expression<Func<OutageEntity, bool>> IsSatisfiedBy => x => x.OutageState == _state;
    }
}
