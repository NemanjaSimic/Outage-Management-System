using Common.OMS.OutageDatabaseModel;
using OMS.HistoryDBManagerServiceImplementation.Reporting.Queries.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OMS.HistoryDBManagerServiceImplementation.Reporting.Queries
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
