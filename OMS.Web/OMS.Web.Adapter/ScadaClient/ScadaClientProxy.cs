using System;
using SCADA_Common;
using System.ServiceModel;
using OMS.Web.Adapter.Contracts;

namespace OMS.Web.Adapter.ScadaClient
{
    public class ScadaClientProxy : ChannelFactory<ICommandService>, IScadaClient
    {
        private ICommandService _proxy = null;

        public ScadaClientProxy(string address) : base(binding: new NetTcpBinding(), remoteAddress: address)
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
                _proxy.RecvCommand(gid, default(PointType), value);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
