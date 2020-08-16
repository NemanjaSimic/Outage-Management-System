using Common.OMS.OutageDatabaseModel;
using OMS.HistoryDBManagerImplementation.Reporting.Queries.Specifications;
using System;
using System.Linq.Expressions;

namespace OMS.HistoryDBManagerImplementation.Reporting.Queries
{
    public class HistoricalConsumerElementIdQuery : HistoricalConsumerSpecification
	{
		private readonly long _id;

		public HistoricalConsumerElementIdQuery(long id)
			=> _id = id;

		public override Expression<Func<ConsumerHistorical, bool>> IsSatisfiedBy
			=> x => x.ConsumerId == _id;
	}
}
