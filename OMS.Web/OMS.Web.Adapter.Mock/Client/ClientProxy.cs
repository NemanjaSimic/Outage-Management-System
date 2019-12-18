using System;
using OMS.Web.UI.Models.ViewModels;
using System.ServiceModel;
using OMS.Web.Adapter.Contracts;
using System.Collections.Generic;

namespace OMS.Web.Adapter.Mock.Client
{
    public class ClientProxy : ChannelFactory<IWebService>, IWebService
    {
        private IWebService _proxy = null;

        public ClientProxy(string address) : base(binding: new NetTcpBinding(), remoteAddress: address)
        {
            _proxy = CreateChannel();
        }

        public void UpdateGraph(List<Node> nodes, List<Relation> relations)
        {
            try
            {
                _proxy.UpdateGraph(nodes, relations);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
