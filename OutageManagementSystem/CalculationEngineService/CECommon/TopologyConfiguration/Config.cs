using Outage.Common;
using System;
using System.IO;

namespace CECommon.TopologyConfiguration
{
	public class Config
	{
		ILogger logger = LoggerWrapper.Instance;
        #region Singleton
        private static object syncObj = new object();
		private static Config instance;
		public static Config Instance
		{
			get
			{
				lock (syncObj)
				{
					if (instance == null)
					{
						instance = new Config();
					}
				}
				return instance;
			}
		}
        #endregion
        private Config(){}
		public string ReadConfiguration(string path)
		{
			string retValue = "";

			try
			{
				using (TextReader sr = new StreamReader(path))
				{
					retValue = sr.ReadToEnd();
				}
			}
			catch (Exception ex)
			{
				string message = $"Failed to read configuration file on path {path}. Exception message: " + ex.Message;
				logger.LogError(message);
				throw ex;
			}

			return retValue;
		}
	}
}
