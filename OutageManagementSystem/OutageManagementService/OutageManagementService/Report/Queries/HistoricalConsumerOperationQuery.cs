namespace OutageManagementService.Report.Queries
{
    using global::Outage.Common;
    using OMSCommon.OutageDatabaseModel;
    using System;
    using System.Linq.Expressions;

    public class HistoricalConsumerOperationQuery : HistoricalConsumerSpecification
    {
        private readonly DatabaseOperation _operation;

        public HistoricalConsumerOperationQuery(DatabaseOperation operation)
            => _operation = operation;

        public override Expression<Func<ConsumerHistorical, bool>> IsSatisfiedBy()
            => x => x.DatabaseOperation == _operation;
    }
}
