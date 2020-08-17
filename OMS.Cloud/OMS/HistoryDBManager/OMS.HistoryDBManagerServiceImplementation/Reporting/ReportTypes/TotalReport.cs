using Common.OMS.OutageDatabaseModel;
using Common.OMS.Report;
using OMS.HistoryDBManagerImplementation.Reporting.Queries;
using OMS.HistoryDBManagerImplementation.Reporting.Queries.Specifications;
using OutageDatabase;
using OutageDatabase.Repository;
using System.Collections.Generic;
using System.Linq;

namespace OMS.HistoryDBManagerImplementation.Reporting.ReportTypes
{
    public class TotalReport : IReport
    {
        private readonly OutageContext _context;
        private readonly OutageRepository _outageRepository;

        public TotalReport()
        {
            _context = new OutageContext();
            _outageRepository = new OutageRepository(_context);
        }

        public OutageReport Generate(ReportOptions options)
        {
            List<Specification<OutageEntity>> specs = new List<Specification<OutageEntity>>();

            if (options.ElementId.HasValue)
                specs.Add(new OutageElementIdQuery(options.ElementId.Value));

            if (options.StartDate.HasValue)
                specs.Add(new OutageStartDateQuery(options.StartDate.Value));

            if (options.EndDate.HasValue)
                specs.Add(new OutageEndDateQuery(options.EndDate.Value));

            IEnumerable<OutageEntity> outages;

            if (specs.Count > 1)
            {
                AndSpecification<OutageEntity> andQuery = new AndSpecification<OutageEntity>(specs);
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

            var outageReportGrouping = outages.GroupBy(o => type == "Monthly" ? o.ReportTime.Month : o.ReportTime.Year).Select(o => o).ToList();

            var reportData = new Dictionary<string, float>();
            foreach (var outage in outageReportGrouping)
                reportData.Add(type == "Monthly" ? DateHelpers.Months[outage.Key] : outage.Key.ToString(), outage.Count());

            return new OutageReport
            {
                Type = type,
                Data = reportData
            };
        }
    }
}
