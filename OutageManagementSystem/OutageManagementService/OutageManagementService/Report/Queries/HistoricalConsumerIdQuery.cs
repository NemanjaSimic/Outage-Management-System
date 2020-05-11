namespace OutageManagementService.Report.Queries
{
    using OMSCommon.OutageDatabaseModel;
    using System;
    using System.Linq.Expressions;

    public class HistoricalConsumerIdQuery : HistoricalConsumerSpecification
    {
        private readonly long _id;

        public HistoricalConsumerIdQuery(long id)
            => _id = id;

		public override Expression<Func<ConsumerHistorical, bool>> IsSatisfiedBy => x => x.ConsumerId == _id;
	}
}
