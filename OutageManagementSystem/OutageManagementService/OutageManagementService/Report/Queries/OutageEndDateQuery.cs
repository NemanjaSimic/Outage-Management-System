namespace OutageManagementService.Report.Queries
{
    using OMSCommon.OutageDatabaseModel;
    using System;
    using System.Linq.Expressions;

    public class OutageEndDateQuery : OutageSpecification
    {
        private readonly DateTime _endDate;

        public OutageEndDateQuery(DateTime endDate)
            => _endDate = endDate;

		public override Expression<Func<OutageEntity, bool>> IsSatisfiedBy => x => x.ReportTime <= _endDate;
	}
}
