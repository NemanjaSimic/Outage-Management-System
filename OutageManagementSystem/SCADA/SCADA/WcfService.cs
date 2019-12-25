using System;
using System.ServiceModel;

namespace Outage.SCADA.SCADA
{
    [Obsolete]
    public class WcfService<TContract, TService> : IDisposable
        where TContract : class
        where TService : TContract
    {
        private ServiceHost _svc = new ServiceHost(typeof(TService));

        public WcfService(string address)
        {
            _svc.AddServiceEndpoint(typeof(TContract), new NetTcpBinding(), address);
        }

        public void Open()
        {
            try
            {
                _svc.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("_svc host failed to open with error: {0}", ex.Message);
            }
        }

        public void Close()
        {
            try
            {
                _svc.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("_svc host failed to close with error: {0}", ex.InnerException);
            }
        }

        public void Dispose()
        {
            ((IDisposable)_svc).Dispose();
        }

        ~WcfService()
        {
            Dispose();
        }
    }
}