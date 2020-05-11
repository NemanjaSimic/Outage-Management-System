namespace OutageManagementService.Report.Queries
{
    using OMSCommon.OutageDatabaseModel;
    using System;
    using System.Linq.Expressions;

    public class OutageElementIdQuery : OutageSpecification
    {
        private readonly long _id;

        public OutageElementIdQuery(long id)
            => _id = id;

        public override Expression<Func<OutageEntity, bool>> IsSatisfiedBy => x => x.OutageElementGid == _id;

    }
}
