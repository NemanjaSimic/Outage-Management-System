using System;
using System.Configuration;
using System.Fabric;

namespace NMS.GdaImplementation
{
    public class Config
    {
        private string dBConnectionString = string.Empty;
        private static StatelessServiceContext context;

        public static StatelessServiceContext Context 
        {
            get { return context; }

            private set
            {
                context = value;
            }
        }

        public string DbConnectionString
        {
            get { return dBConnectionString; }
        }


        private Config()
        {
            var config = Context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            dBConnectionString = config.Settings.Sections["ConnectionStrings"].Parameters["mongoConnectionString"].Value;
        }

        #region Static members

        private static Config instance = null;

        public static Config GetInstance(StatelessServiceContext context = null)
        {
            if(context != null)
            {
                Context = context;
            }

            if (instance == null)
            {
                instance = new Config();
            }

            return instance;
        }

        #endregion Static members
    }
}
