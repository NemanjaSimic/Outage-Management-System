using Common.OMS.Report;
using OutageDatabase;
using OutageDatabase.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.ReportingServiceImplementation.ReportTypes
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
