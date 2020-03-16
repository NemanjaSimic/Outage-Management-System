namespace OutageManagementService.Report.Queries
{
    using OMSCommon.OutageDatabaseModel;
    using System;
    using System.Linq.Expressions;

    public class OutageIdSpecification : OutageSpecification
    {
        private readonly long _id;

        public OutageIdSpecification(long id)
            => _id = id;

        public override Expression<Func<OutageEntity, bool>> IsSatisfiedBy()
            => x => x.OutageId == _id;
        
    }
}
