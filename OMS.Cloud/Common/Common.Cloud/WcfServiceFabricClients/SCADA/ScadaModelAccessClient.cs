using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Fabric;
using System.ServiceModel;

namespace OMS.Common.Cloud.WcfServiceFabricClients.SCADA
{
    public class ScadaModelAccessClient : WcfSeviceFabricClientBase<IScadaModelAccessContract>, IScadaModelAccessContract
    {
        public ScadaModelAccessClient(WcfCommunicationClientFactory<IScadaModelAccessContract> clientFactory, Uri serviceName)
            : base(clientFactory, serviceName)
        {
        }

        public static ScadaModelAccessClient CreateClient(Uri nmsServiceName = null)
        {
            if (nmsServiceName == null && ConfigurationManager.AppSettings[MicroserviceNames.NmsGdaService] is string nmsGdaServiceName)
            {
                nmsServiceName = new Uri(nmsGdaServiceName);
            }

            var partitionResolver = new ServicePartitionResolver(() => new FabricClient());
            //var partitionResolver = ServicePartitionResolver.GetDefault();
            var factory = new WcfCommunicationClientFactory<IScadaModelAccessContract>(CreateBinding(), null, partitionResolver);

            return new ScadaModelAccessClient(factory, nmsServiceName);
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

        #region IScadaModelAccessContract
        public Dictionary<PointType, Dictionary<ushort, long>> GetAddressToGidMapping()
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}
