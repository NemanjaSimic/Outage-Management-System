//namespace OMS.Web.Adapter.WebService
//{
//    using System;
//    using System.ServiceModel;
//    using OMS.Web.Adapter.Contracts;
//    using OMS.Web.Common;

//    public class WebServiceHost
//    {
//        private ServiceHost _host;
//        private readonly string _address;
        
//        public WebServiceHost(string address)
//        {
//            _host = null;
//            _address = address;
//        }

//        public void Open()
//        {
//            NetTcpBinding binding = new NetTcpBinding();

//            _host = new ServiceHost(typeof(WebService));
//            _host.AddServiceEndpoint(typeof(IWebService), binding, _address);

//            try
//            {
//                _host.Open();
//                Console.WriteLine($"Web Service Host opened on: {_address}");
//            }
//            catch (Exception)
//            {
//                throw;
//            }
//        }
//    }
//}
