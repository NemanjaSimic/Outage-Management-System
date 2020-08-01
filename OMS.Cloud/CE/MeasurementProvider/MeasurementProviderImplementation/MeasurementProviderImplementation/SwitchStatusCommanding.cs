using Common.CeContracts;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.WcfClient.CE;
using System;
using System.Threading.Tasks;

namespace MeasurementProviderImplementation
{
	public class SwitchStatusCommanding : ISwitchStatusCommandingContract
	{
		private readonly string baseLogString;

		private readonly MeasurementProviderClient measurementProviderClient;

		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

		public SwitchStatusCommanding()
		{
			this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
			string verboseMessage = $"{baseLogString} entering Ctor.";
			Logger.LogVerbose(verboseMessage);

			measurementProviderClient = MeasurementProviderClient.CreateClient();

			string debugMessage = $"{baseLogString} Ctor => Clients initialized.";
			Logger.LogDebug(debugMessage);
		}
		public async Task SendCloseCommand(long gid)
		{
			string verboseMessage = $"{baseLogString} entering SendCloseCommand method for GID {gid:X16}.";
			Logger.LogVerbose(verboseMessage);

			try
			{
				await measurementProviderClient.SendDiscreteCommand(gid, (int)DiscreteCommandingType.CLOSE, CommandOriginType.USER_COMMAND);
			}
			catch (Exception e)
			{
				string message = $"{baseLogString} SendCloseCommand => " +
					$"Failed to call send discrete command method measurement provider client." +
					$"{Environment.NewLine} Exception message: {e.Message}";
				Logger.LogError(message);
				throw;
			}
		}

		public async Task SendOpenCommand(long gid)
		{
			string verboseMessage = $"{baseLogString} entering SendOpenCommand method for GID {gid:X16}.";
			Logger.LogVerbose(verboseMessage);

			try
			{
				await measurementProviderClient.SendDiscreteCommand(gid, (int)DiscreteCommandingType.OPEN, CommandOriginType.USER_COMMAND);
			}
			catch (Exception e)
			{
				string message = $"{baseLogString} GetElementToMeasurementMap => " +
						$"Failed to Failed to call send discrete command method from measurement provider client." +
						$"{Environment.NewLine} Exception message: {e.Message}";
				Logger.LogError(message);
				throw;
			}
		}
	}
}
