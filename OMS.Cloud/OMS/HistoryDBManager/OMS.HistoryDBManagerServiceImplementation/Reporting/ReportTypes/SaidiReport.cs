using Common.OmsContracts.DataContracts.Report;
using OutageDatabase;
using OutageDatabase.Repository;
using System;

namespace OMS.HistoryDBManagerImplementation.Reporting.ReportTypes
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
