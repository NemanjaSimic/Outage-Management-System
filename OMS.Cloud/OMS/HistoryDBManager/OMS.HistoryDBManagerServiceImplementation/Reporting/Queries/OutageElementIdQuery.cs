using Common.OMS.OutageDatabaseModel;
using OMS.HistoryDBManagerImplementation.Reporting.Queries.Specifications;
using System;
using System.Linq.Expressions;

namespace OMS.HistoryDBManagerImplementation.Reporting.Queries
{
    public class OutageElementIdQuery : OutageSpecification
    {
        private readonly long _id;

        public OutageElementIdQuery(long id)
            => _id = id;

        public override Expression<Func<OutageEntity, bool>> IsSatisfiedBy => x => x.OutageElementGid == _id;

    }
}
