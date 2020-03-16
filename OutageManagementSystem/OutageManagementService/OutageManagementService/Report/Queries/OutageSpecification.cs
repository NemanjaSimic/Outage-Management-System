namespace OutageManagementService.Report.Queries
{
    using OMSCommon.OutageDatabaseModel;
    using System;
    using System.Linq.Expressions;

    public abstract class OutageSpecification : Specification<OutageEntity>
    {
        public abstract override Expression<Func<OutageEntity, bool>> IsSatisfiedBy();
    }
}
