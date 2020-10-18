using Microsoft.AspNetCore.SignalR.Client;
using OMS.Common.Cloud.Logger;
using System;
using System.Threading.Tasks;

namespace WebAdapterImplementation.HubDispatchers
{
	public class OutageEmailHubDispatcher
	{
		private const string outageHubUrl = "http://localhost:44351/graphhub";

		private readonly string baseLogString;
		private readonly HubConnection connection;

		#region Private Properties
		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}
		#endregion Private Properties

		public OutageEmailHubDispatcher()
		{
			this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
			this.connection = new HubConnectionBuilder().WithUrl(outageHubUrl)
														.Build();
		}

		public void Connect()
		{
			this.connection.StartAsync().ContinueWith(task =>
			{
				if (task.IsFaulted)
				{
					string message = $"{baseLogString} Connect => Fault on connection.";
					Logger.LogError(message);
				}
				else
				{
					string message = $"{baseLogString} Connect => Hub Successfully connected. url: {outageHubUrl}";
					Logger.LogDebug(message);
				}
			}).Wait();
		}

		public async Task NotifyGraphOutageCall(long gid)
		{
			try
			{
				Logger.LogDebug($"{baseLogString} NotifyGraphOutageCall => updating outage call on consumer with gid: 0x{gid:X16}");

				await this.connection.InvokeAsync("NotifyGraphOutageCall", gid);

				Logger.LogDebug($"{baseLogString} NotifyGraphOutageCall => json output sent to outage hub: {gid}");
			}
			catch (Exception e)
			{
				Logger.LogError($"{baseLogString} NotifyGraphOutageCall => Exception: {e.Message}", e);
			}
		}
	}
}
