using Common.OMS.Report;
using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using Common.OmsContracts.DataContracts.Report;
using OMS.Common.Cloud;
using OMS.HistoryDBManagerImplementation.Reporting.Queries;
using OMS.HistoryDBManagerImplementation.Reporting.Queries.Specifications;
using OutageDatabase;
using OutageDatabase.Repository;
using System.Collections.Generic;
using System.Linq;

namespace OMS.HistoryDBManagerImplementation.Reporting.ReportTypes
{
    public class SaifiReport : IReport
    {
        private readonly OutageContext _context;
        private readonly ConsumerHistoricalRepository _outageRepository;
        private readonly ConsumerRepository _consumerRepository;

        public SaifiReport()
        {
            _context = new OutageContext();
            _outageRepository = new ConsumerHistoricalRepository(_context);
            _consumerRepository = new ConsumerRepository(_context);
        }

        public OutageReport Generate(ReportOptions options)
        {
            bool isScope = false;
            List<Specification<ConsumerHistorical>> specs = new List<Specification<ConsumerHistorical>>();

            if (options.StartDate.HasValue)
                specs.Add(new HistoricalConsumerStartDateQuery(options.StartDate.Value));

            if (options.EndDate.HasValue)
                specs.Add(new HistoricalConsumerEndDateQuery(options.EndDate.Value));

            if (options.ElementId != null)
            {
                specs.Add(new HistoricalConsumerElementIdQuery((long)options.ElementId));
                isScope = true;
            }

            specs.Add(new HistoricalConsumerOperationQuery(DatabaseOperation.DELETE));

            IEnumerable<ConsumerHistorical> outages;

            if (specs.Count > 1)
            {
                AndSpecification<ConsumerHistorical> andQuery = new AndSpecification<ConsumerHistorical>(specs);
                outages = _outageRepository.Find(andQuery.IsSatisfiedBy).ToList();
            }
            else if (specs.Count == 1)
            {
                outages = _outageRepository.Find(specs[0].IsSatisfiedBy).ToList();
            }
            else
            {
                outages = _outageRepository.GetAll().ToList();
            }

            var type = DateHelpers.GetType(options.StartDate, options.EndDate);
            var outageReportGrouping = outages.GroupBy(o => type == "Monthly" ? o.OperationTime.Month : o.OperationTime.Year).Select(o => o).ToList();

            var numOfConsumers = 1;

            if (!isScope)
            {
                numOfConsumers = _consumerRepository.GetAll().Count();
            }

            var reportData = new Dictionary<string, float>();
            foreach (var outage in outageReportGrouping)
            {
                var outageCount = outage.Count();
                reportData.Add(type == "Monthly" ? DateHelpers.Months[outage.Key] : outage.Key.ToString(), (float)outageCount / (float)numOfConsumers); ;
            }

            return new OutageReport
            {
                Type = type,
                Data = reportData
            };
        }
    }
}
