using OMS.Common.Cloud.Logger;
using System;
using System.IO;

namespace CE.TopologyBuilderImplementation.Configuration
{
	public class Config
	{
		private readonly string baseLogString;

		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

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
        private Config()
		{
			this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
			string verboseMessage = $"{baseLogString} entering Ctor.";
			Logger.LogVerbose(verboseMessage);
		}
		public string ReadConfiguration(string path)
		{
			string verboseMessage = $"{baseLogString} ReadConfiguration method called for file {path}.";
			Logger.LogVerbose(verboseMessage);
			
			string retValue = "";

			try
			{
				Logger.LogDebug($"{baseLogString} ReadConfiguration => Starting stream reader and reading file {path} to the end.");
				using (TextReader sr = new StreamReader(path))
				{
					retValue = sr.ReadToEnd();
				}
			}
			catch (Exception e)
			{
				string message = 
					$"Failed to read configuration file {path}." +
					$"{Environment.NewLine} Exception message: {e.Message}." +
					$"{Environment.NewLine} Stack trace: {e.StackTrace}";
				Logger.LogError(message);
				throw new Exception(message);
			}
			return retValue;
		}
	}
}
