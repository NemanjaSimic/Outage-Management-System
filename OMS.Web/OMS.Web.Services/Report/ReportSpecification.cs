namespace OMS.Web.Services.Report
{
    using System;
    using System.Linq.Expressions;

    public abstract class ReportSpecification<T> where T : class
    {
        public abstract Expression<Func<T, bool>> IsSatisfiedBy();
    }
}
