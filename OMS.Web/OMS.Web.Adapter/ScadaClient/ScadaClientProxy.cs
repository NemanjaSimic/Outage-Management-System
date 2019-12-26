using System;
using OMS.Web.Common;
using System.ServiceModel;
using OMS.Web.Adapter.Contracts;
using Outage.Common.ServiceContracts;

namespace OMS.Web.Adapter.ScadaClient
{
    public class ScadaClientProxy : ChannelFactory<ISCADACommand>, IScadaClient
    {
        private ISCADACommand _proxy = null;

        public ScadaClientProxy() : base(binding: new NetTcpBinding(), remoteAddress: AppSettings.Get<string>("scadaServiceAddress"))
        {
            _proxy = CreateChannel();
        }

        /// <summary>
        /// Sends command to SCADA WCF Service
        /// </summary>
        /// <param name="gid">Element GUID</param>
        /// <param name="value">New element value</param>
        public void SendCommand(long gid, object value)
        {
            try
            {
                // magic :))
                _proxy.RecvCommand(gid, value);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
