namespace OutageManagementService.Report.Queries
{
    using System;
    using System.Linq.Expressions;

    public abstract class Specification<T> where T : class
    {
        public abstract Expression<Func<T, bool>> IsSatisfiedBy();
    }
}
