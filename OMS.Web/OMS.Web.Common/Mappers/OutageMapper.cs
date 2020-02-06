namespace OMS.Web.Common.Mappers
{
    using OMS.Web.UI.Models.ViewModels;
    using Outage.Common.PubSub.OutageDataContract;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class OutageMapper : IOutageMapper
    {
        private IConsumerMapper _consumerMapper;

        public OutageMapper(IConsumerMapper consumerMapper)
        {
            _consumerMapper = consumerMapper;
        }

        public ActiveOutageViewModel MapActiveOutage(ActiveOutage outage)
            => new ActiveOutageViewModel
            {
                Id = outage.OutageId,
                ReportedAt = outage.ReportTime,
                ElementId = outage.ElementGid,
                AffectedConsumers = _consumerMapper.MapConsumers(outage.AffectedConsumers)
            };

        public IEnumerable<ActiveOutageViewModel> MapActiveOutages(IEnumerable<ActiveOutage> outages)
            => outages.Select(o => MapActiveOutage(o)).ToList();

        public ArchivedOutageViewModel MapArchivedOutage(ArchivedOutage outage)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ArchivedOutageViewModel> MapArchivedOutages(IEnumerable<ArchivedOutage> outages)
        {
            throw new NotImplementedException();
        }
    }
}
