using Common.OMS.OutageDatabaseModel;
using Common.PubSubContracts.DataContracts.OMS;
using Common.Web.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace Common.Web.Mappers
{
    public class ConsumerMapper : IConsumerMapper
    {
        //TODO:
        //private IOutageMapper _outageMapper;

        //public ConsumerMapper(IOutageMapper outageMapper)
        //{
        //    //_outageMapper = outageMapper;
        //}

        public ConsumerViewModel MapConsumer(Consumer consumer)
            => new ConsumerViewModel
            {
                Id = consumer.ConsumerId,
                Mrid = consumer.ConsumerMRID,
                FirstName = consumer.FirstName,
                LastName = consumer.LastName,
                ActiveOutages = new List<ActiveOutageViewModel>(),      //TODO: _outageMapper.MapActiveOutages(consumer.ActiveOutages),
                ArchivedOutages = new List<ArchivedOutageViewModel>(),  //TODO: _outageMapper.MapArchivedOutages(consumer.ArchivedOutages)
            };

        public ConsumerViewModel MapConsumer(ConsumerMessage consumer)
            => new ConsumerViewModel
            {
                Id = consumer.ConsumerId,
                Mrid = consumer.ConsumerMRID,
                FirstName = consumer.FirstName,
                LastName = consumer.LastName,
                ActiveOutages = new List<ActiveOutageViewModel>(),      //TODO: _outageMapper.MapActiveOutages(consumer.ActiveOutages),
                ArchivedOutages = new List<ArchivedOutageViewModel>(),  //TODO: _outageMapper.MapArchivedOutages(consumer.ArchivedOutages)
            };

        public IEnumerable<ConsumerViewModel> MapConsumers(IEnumerable<Consumer> consumers)
            => consumers.Select(c => MapConsumer(c)).ToList();

        public IEnumerable<ConsumerViewModel> MapConsumers(IEnumerable<ConsumerMessage> consumers)
            => consumers.Select(c => MapConsumer(c)).ToList();
    }
}
