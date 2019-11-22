using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.NetworkModelService
{
    public class Config
    {
        private string connectionString = string.Empty;

        public string ConnectionString
        {
            get { return connectionString; }
        }

        private Config()
        {
            connectionString = ConfigurationManager.ConnectionStrings["networkModelconnectionString"].ConnectionString;
        }

        #region Static members

        private static Config instance = null;

        public static Config Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Config();
                }

                return instance;
            }
        }

        #endregion Static members

        public string GetCompositeId(long valueWithSystemId)
        {
            string systemId = (Math.Abs(valueWithSystemId) >> 48).ToString();
            string valueWithoutSystemId = (Math.Abs(valueWithSystemId) & 0x0000FFFFFFFFFFFF).ToString();

            return String.Format("{0}{1}.{2}", valueWithSystemId < 0 ? "-" : "", systemId, valueWithoutSystemId);
        }
    }
}
