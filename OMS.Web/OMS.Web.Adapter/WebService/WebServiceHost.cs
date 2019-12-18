using OMS.Web.Adapter.Contracts;
using OMS.Web.Common;
using System;
using System.ServiceModel;

namespace OMS.Web.Adapter.WebService
{
    public class WebServiceHost
    {
        private ServiceHost _host;
        
        public WebServiceHost()
        {
            _host = null;
        }

        public void Open()
        {
            NetTcpBinding binding = new NetTcpBinding();
            string address = AppSettings.Get<string>("webServiceAddress");

            _host = new ServiceHost(typeof(WebService));
            _host.AddServiceEndpoint(typeof(IWebService), binding, address);

            try
            {
                _host.Open();
                Console.WriteLine($"Web Service Host opened on: {address}");
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
