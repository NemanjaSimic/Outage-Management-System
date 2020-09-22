using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using Common.OmsContracts.ModelAccess;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using System;
using System.Collections.Generic;
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

        public static IEquipmentAccessContract CreateClient()
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<EquipmentAccessClient, IEquipmentAccessContract>(microserviceName);
        }

        public static IEquipmentAccessContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<EquipmentAccessClient, IEquipmentAccessContract>(serviceUri, servicePartitionKey);
        }

		#region IEquipmentAccessContract
		public Task<Equipment> AddEquipment(Equipment equipment)
		{
			return InvokeWithRetryAsync(client => client.Channel.AddEquipment(equipment));
		}

        //public Task<IEnumerable<Equipment>> FindEquipment(EquipmentExpression expression)
        //{
        //    return InvokeWithRetryAsync(client => client.Channel.FindEquipment(expression));
        //}

        public Task<List<Equipment>> GetAllEquipments()
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

		public Task<bool> IsAlive()
		{
			return InvokeWithRetryAsync(client => client.Channel.IsAlive());
		}
		#endregion IEquipmentAccessContract
	}
}
