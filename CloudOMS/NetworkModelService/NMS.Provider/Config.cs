﻿using System;
using System.Configuration;
using System.Fabric;

namespace CloudOMS.NetworkModelService.NMS.Provider
{
    public class Config
    {
        private string dBConnectionString = string.Empty;
        private StatelessServiceContext context;

        public StatelessServiceContext Context 
        {
            get { return context; }

            set
            {
                context = value;
                Set();
            }
        }

        public string DbConnectionString
        {
            get { return dBConnectionString; }
        }


        private Config()
        {
            //dBConnectionString = ConfigurationManager.ConnectionStrings["mongoConnectionString"].ConnectionString;
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

        private void Set()
        {
            var config = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            dBConnectionString = config.Settings.Sections["ConnectionStrings"].Parameters["mongoConnectionString"].Value;
        }

        public string GetCompositeId(long valueWithSystemId)
        {
            string systemId = (Math.Abs(valueWithSystemId) >> 48).ToString();
            string valueWithoutSystemId = (Math.Abs(valueWithSystemId) & 0x0000FFFFFFFFFFFF).ToString();

            return String.Format("{0}{1}.{2}", valueWithSystemId < 0 ? "-" : "", systemId, valueWithoutSystemId);
        }
    }
}
