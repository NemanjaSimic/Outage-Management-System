using Common.OMS.OutageDatabaseModel;
using Common.OmsContracts.ModelAccess;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.OMS.ModelAccess
{
	public class EquipmentAccessClient : WcfSeviceFabricClientBase<IEquipmentAccessContract>, IEquipmentAccessContract
	{
        private static readonly string microserviceName = MicroserviceNames.OmsHistoryDBManagerService;
        private static readonly string listenerName = EndpointNames.OmsEquipmentAccessEndpoint;
        public EquipmentAccessClient(WcfCommunicationClientFactory<IEquipmentAccessContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
           : base(clientFactory, serviceUri, servicePartition, listenerName)
        {

        }
        public static EquipmentAccessClient CreateClient()
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<EquipmentAccessClient, IEquipmentAccessContract>(microserviceName);
        }

        public static EquipmentAccessClient CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<EquipmentAccessClient, IEquipmentAccessContract>(serviceUri, servicePartitionKey);
        }

		public Task<Equipment> AddEquipment(Equipment equipment)
		{
			return InvokeWithRetryAsync(client => client.Channel.AddEquipment(equipment));
		}

		public Task<IEnumerable<Equipment>> FindEquipment(Expression<Func<Equipment, bool>> predicate)
		{
			return InvokeWithRetryAsync(client => client.Channel.FindEquipment(predicate));
		}

		public Task<IEnumerable<Equipment>> GetAllEquipments()
		{
			return InvokeWithRetryAsync(client => client.Channel.GetAllEquipments());
		}

		public Task<Equipment> GetEquipment(long gid)
		{
			return InvokeWithRetryAsync(client => client.Channel.GetEquipment(gid));
		}

		public Task RemoveAllEquipments()
		{
			return InvokeWithRetryAsync(client => client.Channel.RemoveAllEquipments());
		}

		public Task RemoveEquipment(Equipment equipment)
		{
			return InvokeWithRetryAsync(client => client.Channel.RemoveEquipment(equipment));
		}

		public Task UpdateEquipment(Equipment equipment)
		{
			return InvokeWithRetryAsync(client => client.Channel.UpdateEquipment(equipment));
		}
	}
}
