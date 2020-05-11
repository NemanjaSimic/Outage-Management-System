namespace OutageManagementService.Report.Queries
{
    using global::Outage.Common;
    using OMSCommon.OutageDatabaseModel;
    using System;
    using System.Linq.Expressions;

    public class OutageLifecycleQuery : OutageSpecification
    {
        private readonly OutageState _state;

        public OutageLifecycleQuery(OutageState state)
            => _state = state;

        public override Expression<Func<OutageEntity, bool>> IsSatisfiedBy()
            => x => x.OutageState == _state;
    }
}
