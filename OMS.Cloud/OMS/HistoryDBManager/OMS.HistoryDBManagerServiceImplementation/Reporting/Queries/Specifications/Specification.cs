using System;
using System.Linq.Expressions;

namespace OMS.HistoryDBManagerImplementation.Reporting.Queries.Specifications
{
    public abstract class Specification<T> where T : class
    {
        public abstract Expression<Func<T, bool>> IsSatisfiedBy { get; }
    }
}
