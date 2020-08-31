using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using OMS.Common.Cloud;
using OMS.HistoryDBManagerImplementation.Reporting.Queries.Specifications;
using System;
using System.Linq.Expressions;

namespace OMS.HistoryDBManagerImplementation.Reporting.Queries
{
    public class HistoricalConsumerOperationQuery : HistoricalConsumerSpecification
    {
        private readonly DatabaseOperation _operation;

        public HistoricalConsumerOperationQuery(DatabaseOperation operation)
            => _operation = operation;

        public override Expression<Func<ConsumerHistorical, bool>> IsSatisfiedBy => x => x.DatabaseOperation == _operation;
    }
}
