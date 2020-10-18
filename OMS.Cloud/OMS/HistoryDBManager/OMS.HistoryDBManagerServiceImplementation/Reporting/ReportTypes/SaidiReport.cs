using Common.OMS.Report;
using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using Common.OmsContracts.DataContracts.Report;
using OMS.Common.Cloud;
using OMS.HistoryDBManagerImplementation.Reporting.Queries;
using OMS.HistoryDBManagerImplementation.Reporting.Queries.Specifications;
using OutageDatabase;
using OutageDatabase.Repository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OMS.HistoryDBManagerImplementation.Reporting.ReportTypes
{
    public class SaidiReport : IReport
    {
        private readonly OutageContext _context;
        private readonly ConsumerHistoricalRepository _outageRepository;
        private readonly ConsumerRepository _consumerRepository;

        public SaidiReport()
        {
            _context = new OutageContext();
            _outageRepository = new ConsumerHistoricalRepository(_context);
            _consumerRepository = new ConsumerRepository(_context);
        }

        public OutageReport Generate(ReportOptions options)
        {
            List<Specification<ConsumerHistorical>> specs = new List<Specification<ConsumerHistorical>>();

            if (options.StartDate.HasValue)
                specs.Add(new HistoricalConsumerStartDateQuery(options.StartDate.Value));

            if (options.EndDate.HasValue)
                specs.Add(new HistoricalConsumerEndDateQuery(options.EndDate.Value));

            if (options.ElementId != null)
            {
                specs.Add(new HistoricalConsumerElementIdQuery((long)options.ElementId));
            }

            IEnumerable<ConsumerHistorical> consumers;

            if (specs.Count > 1)
            {
                AndSpecification<ConsumerHistorical> andQuery = new AndSpecification<ConsumerHistorical>(specs);
                consumers = _outageRepository.Find(andQuery.IsSatisfiedBy).ToList();
            }
            else if (specs.Count == 1)
            {
                consumers = _outageRepository.Find(specs[0].IsSatisfiedBy).ToList();
            }
            else
            {
                consumers = _outageRepository.GetAll().ToList();
            }

            var sumirani = new List<Tuple<DateTime, float>>();
            var insert = consumers.Where(c => c.DatabaseOperation == DatabaseOperation.INSERT).OrderBy(c => c.OperationTime).ToList();
            var delete = consumers.Where(c => c.DatabaseOperation == DatabaseOperation.DELETE).OrderBy(c => c.OperationTime).ToList();

            foreach (var item in insert)
            {
                var consumer = delete.FirstOrDefault(c => c.ConsumerId == item.ConsumerId);
                if (consumer != null)
                {
                    sumirani.Add(new Tuple<DateTime, float>(consumer.OperationTime ,(float)((consumer.OperationTime - item.OperationTime).TotalMinutes)));
                    delete.Remove(consumer);
                }
            }

            var reportData = new Dictionary<string, float>();
            var type = DateHelpers.GetType(options.StartDate, options.EndDate);
            List<IGrouping<int,Tuple<DateTime, float>>> outageReportGrouping = null;

            if (type == "Yearly")
            {
                outageReportGrouping = sumirani.GroupBy(o => o.Item1.Month).Select(o => o).ToList();

            }
            else if (type == "Monthly")
            {
                outageReportGrouping = sumirani.GroupBy(o => o.Item1.Day).Select(o => o).ToList();

            }
            else
            {
                outageReportGrouping = sumirani.GroupBy(o => o.Item1.Hour).Select(o => o).ToList();
            }

            var numOfConsumers = _consumerRepository.GetAll().Count();

            foreach (var outage in outageReportGrouping)
            {
                var outageTime = outage.ToList().Sum(c => c.Item2);
                reportData.Add(type == "Yearly" ? DateHelpers.Months[outage.Key] : outage.Key.ToString(), (float)outageTime / (float)numOfConsumers);
            }

            return new OutageReport
            {
                Type = type,
                Data = reportData
            };
        }


    }
}
