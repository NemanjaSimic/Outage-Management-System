using OMSCommon.OutageDatabaseModel;
using Outage.Common.OutageService;
using OutageDatabase;
using OutageDatabase.Repository;
using OutageManagementService.Report.Queries;
using System.Collections.Generic;

namespace OutageManagementService.Report
{
    public class TotalReport : IReport
    {
        // posto ne postoji nijedan interface za repozitorijume
        // nema ni DI
        private readonly OutageContext _context;
        private readonly OutageRepository _outageRepository;
        private readonly ConsumerHistoricalRepository _consumerHistoricalRepository;

        public TotalReport()
        {
            _context = new OutageContext();
            _outageRepository = new OutageRepository(_context);
            _consumerHistoricalRepository = new ConsumerHistoricalRepository(_context);
        }

        public OutageReport Generate(ReportOptions options)
        {
            List<Specification<OutageEntity>> specs = new List<Specification<OutageEntity>>();

            if (options.ElementId.HasValue)
                specs.Add(new OutageIdQuery(options.ElementId.Value));

            if (options.StartDate.HasValue)
                specs.Add(new OutageStartDateQuery(options.StartDate.Value));
            
            if (options.EndDate.HasValue)
                specs.Add(new OutageStartDateQuery(options.EndDate.Value));

            AndSpecification<OutageEntity> andQuery = new AndSpecification<OutageEntity>(specs);
            
            // ovde treba da vrati ID 9, koji ja imam u bazi, ali nece da pronadje ovako
            // kad odradim GetAll, vrati mi sve i medju njima je bas taj sa ID 9
            var outages = _outageRepository.Get(9);

            return new OutageReport { };
        }
    }
}
