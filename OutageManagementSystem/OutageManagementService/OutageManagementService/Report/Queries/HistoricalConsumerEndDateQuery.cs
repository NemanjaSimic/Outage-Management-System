namespace OutageManagementService.Report.Queries
{
    using OMSCommon.OutageDatabaseModel;
    using System;
    using System.Linq.Expressions;

    public class HistoricalConsumerEndDateQuery : HistoricalConsumerSpecification
    {
        private readonly DateTime _date;

        public HistoricalConsumerEndDateQuery(DateTime date)
            => _date = date;

        public override Expression<Func<ConsumerHistorical, bool>> IsSatisfiedBy()
            => x => x.OperationTime <= _date;
    }
}
