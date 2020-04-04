using Microsoft.ServiceFabric.Services.Communication.Wcf;
using System;
using System.ServiceModel;

namespace OMS.Common.Cloud.WcfServiceFabricClients
{
    public static class TcpBindingHelper
    {
        public static NetTcpBinding CreateListenerBinding()
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

            var binding = WcfUtility.CreateTcpListenerBinding();
            binding.SendTimeout = TimeSpan.MaxValue;
            binding.ReceiveTimeout = TimeSpan.MaxValue;
            binding.OpenTimeout = TimeSpan.FromMinutes(1);
            binding.CloseTimeout = TimeSpan.FromMinutes(1);

            return (NetTcpBinding)binding;
        }

        public static NetTcpBinding CreateClientBinding()
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
    }
}
