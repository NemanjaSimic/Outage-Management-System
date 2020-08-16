using Common.OMS.OutageDatabaseModel;
using System;
using System.Linq.Expressions;

namespace OMS.HistoryDBManagerImplementation.Reporting.Queries.Specifications
{
    public abstract class OutageSpecification : Specification<OutageEntity>
    {
        public abstract override Expression<Func<OutageEntity, bool>> IsSatisfiedBy { get; }
    }
}
