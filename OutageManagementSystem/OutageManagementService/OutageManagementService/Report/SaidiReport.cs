using Outage.Common.OutageService;
using OutageDatabase;
using OutageDatabase.Repository;
using System;
using System.Linq;

namespace OutageManagementService.Report
{
    public class SaidiReport : IReport
    {
        private readonly OutageContext _context;
        private readonly OutageRepository _outageRepository;
        private readonly ConsumerHistoricalRepository _consumerHistoricalRepository;

        public SaidiReport()
        {
            _context = new OutageContext();
            _outageRepository = new OutageRepository(_context);
            _consumerHistoricalRepository = new ConsumerHistoricalRepository(_context);
        }

        public OutageReport Generate(ReportOptions options)
        {
            throw new Exception("SAIDIII");
        }
    }
}
