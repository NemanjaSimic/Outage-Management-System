namespace OutageManagementService.Report.Queries
{
    using OMSCommon.OutageDatabaseModel;
    using System;
    using System.Linq.Expressions;

    public abstract class HistoricalConsumerSpecification : Specification<ConsumerHistorical>
    {
        public abstract override Expression<Func<ConsumerHistorical, bool>> IsSatisfiedBy();
    }
}
