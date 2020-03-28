using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Outage.Common;
using Outage.Common.ServiceContracts.SCADA;
using System;
using System.Configuration;
using System.Fabric;
using System.ServiceModel;

namespace OMS.Common.Cloud.WcfServiceFabricClients.SCADA
{

    public class ScadaCommandingClient : WcfSeviceFabricClientBase<IScadaCommandingContract>, IScadaCommandingContract
    {
        public ScadaCommandingClient(WcfCommunicationClientFactory<IScadaCommandingContract> clientFactory, Uri serviceName)
            : base(clientFactory, serviceName)
        {
        }

        public static ScadaCommandingClient CreateClient(Uri nmsServiceName = null)
        {
            if (nmsServiceName == null && ConfigurationManager.AppSettings[MicroserviceNames.ScadaCommandingService] is string nmsGdaServiceName)
            {
                nmsServiceName = new Uri(nmsGdaServiceName);
            }

            var partitionResolver = new ServicePartitionResolver(() => new FabricClient());
            //var partitionResolver = ServicePartitionResolver.GetDefault();
            var factory = new WcfCommunicationClientFactory<IScadaCommandingContract>(CreateBinding(), null, partitionResolver);

            return new ScadaCommandingClient(factory, nmsServiceName);
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

        #region IScadaCommandingContract
        public bool SendAnalogCommand(long gid, float commandingValue, CommandOriginType commandOriginType)
        {
            throw new System.NotImplementedException();
        }

        public bool SendDiscreteCommand(long gid, ushort commandingValue, CommandOriginType commandOriginType)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}
