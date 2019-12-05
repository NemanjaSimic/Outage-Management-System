using OMS.Web.Adapter.Contracts;
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
            // change to read from configuration
            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://localhost:9990/WebService";

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
