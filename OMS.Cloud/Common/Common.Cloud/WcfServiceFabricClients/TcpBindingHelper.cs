using Microsoft.ServiceFabric.Services.Communication.Wcf;
using System;
using System.ServiceModel;

namespace OMS.Common.Cloud.WcfServiceFabricClients
{
    public static class TcpBindingHelper
    {
        public static NetTcpBinding CreateListenerBinding()
        {
            //NetTcpBinding binding = (NetTcpBinding)WcfUtility.CreateTcpListenerBinding();
            //binding.SendTimeout = TimeSpan.MaxValue;
            //binding.ReceiveTimeout = TimeSpan.MaxValue;
            //binding.OpenTimeout = TimeSpan.FromMinutes(1);
            //binding.CloseTimeout = TimeSpan.FromMinutes(1);
            //binding.MaxBufferSize = (int)binding.MaxReceivedMessageSize;
            //binding.MaxBufferPoolSize = Environment.ProcessorCount * binding.MaxReceivedMessageSize;

            //return binding;
            return CreateBinding();
        }

        public static NetTcpBinding CreateClientBinding()
        {
            //NetTcpBinding binding = (NetTcpBinding)WcfUtility.CreateTcpClientBinding();
            //binding.SendTimeout = TimeSpan.MaxValue;
            //binding.ReceiveTimeout = TimeSpan.MaxValue;
            //binding.OpenTimeout = TimeSpan.FromMinutes(1);
            //binding.CloseTimeout = TimeSpan.FromMinutes(1);
            //binding.MaxBufferSize = (int)binding.MaxReceivedMessageSize;
            //binding.MaxBufferPoolSize = Environment.ProcessorCount * binding.MaxReceivedMessageSize;

            //return binding;
            return CreateBinding();
        }

        private static NetTcpBinding CreateBinding()
        {
            int maxReceivedMessageSize = 1024 * 1024 * 1024;

            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None)
            {
                SendTimeout = TimeSpan.MaxValue,
                ReceiveTimeout = TimeSpan.MaxValue,
                OpenTimeout = TimeSpan.FromMinutes(1),
                CloseTimeout = TimeSpan.FromMinutes(1),
                MaxConnections = int.MaxValue,
                MaxReceivedMessageSize = maxReceivedMessageSize,
                MaxBufferSize = maxReceivedMessageSize,
                MaxBufferPoolSize = Environment.ProcessorCount * maxReceivedMessageSize,
            };

            return binding;
        }

    }
}
