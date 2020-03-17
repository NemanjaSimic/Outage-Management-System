namespace OutageManagementService.Report.Queries
{
    using OMSCommon.OutageDatabaseModel;
    using System;
    using System.Linq.Expressions;

    public class OutageStartDateQuery : OutageSpecification
    {
        private readonly DateTime _startDate;

        public OutageStartDateQuery(DateTime startDate)
            => _startDate = startDate;

        public override Expression<Func<OutageEntity, bool>> IsSatisfiedBy()
            => x => x.ReportTime.Date >= _startDate.Date;
    }
}
