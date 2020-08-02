using Common.OMS.OutageDatabaseModel;
using System;
using System.Linq.Expressions;

namespace OMS.ReportingServiceImplementation.Queries
{
    public class OutageElementIdQuery : OutageSpecification
    {
        private readonly long _id;

        public OutageElementIdQuery(long id)
            => _id = id;

        public override Expression<Func<OutageEntity, bool>> IsSatisfiedBy => x => x.OutageElementGid == _id;

    }
}
