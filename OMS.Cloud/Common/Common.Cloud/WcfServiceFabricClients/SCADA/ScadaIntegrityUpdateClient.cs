using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Outage.Common;
using Outage.Common.PubSub.SCADADataContract;
using Outage.Common.ServiceContracts.SCADA;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Fabric;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Common.Cloud.WcfServiceFabricClients.SCADA
{
    class ScadaIntegrityUpdateClient : WcfSeviceFabricClientBase<IScadaIntegrityUpdateContract>, IScadaIntegrityUpdateContract
    {
        public ScadaIntegrityUpdateClient(WcfCommunicationClientFactory<IScadaIntegrityUpdateContract> clientFactory, Uri serviceName)
            : base(clientFactory, serviceName)
        {
        }

        public static ScadaIntegrityUpdateClient CreateClient(Uri nmsServiceName = null)
        {
            if (nmsServiceName == null && ConfigurationManager.AppSettings[MicroserviceNames.ScadaModelProviderService] is string nmsGdaServiceName)
            {
                nmsServiceName = new Uri(nmsGdaServiceName);
            }

            var partitionResolver = new ServicePartitionResolver(() => new FabricClient());
            //var partitionResolver = ServicePartitionResolver.GetDefault();
            var factory = new WcfCommunicationClientFactory<IScadaIntegrityUpdateContract>(CreateBinding(), null, partitionResolver);

            return new ScadaIntegrityUpdateClient(factory, nmsServiceName);
        }

        private static NetTcpBinding CreateBinding()
        {
            //NetTcpBinding binding = new NetTcpBinding(SecurityMode.None)
            //{
            //    SendTimeout = TimeSpan.MaxValue,
            //    ReceiveTimeout = TimeSpan.MaxValue,
            //    OpenTimeout = TimeSpan.FromMinutes(1),
            //    CloseTimeout = TimeSpan.FromMinutes(1),
            //    MaxConnections = int.MaxValue,
            //    MaxReceivedMessageSize = 1024 * 1024 * 1024,
            //};

            //binding.MaxBufferSize = (int)binding.MaxReceivedMessageSize;
            //binding.MaxBufferPoolSize = Environment.ProcessorCount * binding.MaxReceivedMessageSize;

            var binding = WcfUtility.CreateTcpClientBinding();
            binding.SendTimeout = TimeSpan.MaxValue;
            binding.ReceiveTimeout = TimeSpan.MaxValue;
            binding.OpenTimeout = TimeSpan.FromMinutes(1);
            binding.CloseTimeout = TimeSpan.FromMinutes(1);

            return (NetTcpBinding)binding;
        }

        #region IScadaIntegrityUpdateContract
        public Dictionary<Topic, SCADAPublication> GetIntegrityUpdate()
        {
            throw new NotImplementedException();
        }

        public SCADAPublication GetIntegrityUpdateForSpecificTopic(Topic topic)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
