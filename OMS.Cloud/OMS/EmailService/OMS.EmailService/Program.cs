using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Common.Cloud.Logger;

namespace OMS.EmailService
{
	internal static class Program
	{
		private const string serviceTypeName = "OMS.EmailServiceType";

		private static ICloudLogger logger;
		private static ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

		/// <summary>
		/// This is the entry point of the service host process.
		/// </summary>
		private static void Main()
		{
			string baseLogString = $"{typeof(Program)} [static] =>";

			try
			{
				// The ServiceManifest.XML file defines one or more service type names.
				// Registering a service maps a service type name to a .NET type.
				// When Service Fabric creates an instance of this service type,
				// an instance of the class is created in this host process.

				Logger.LogDebug($"{baseLogString} Main => Calling RegisterServiceAsync for type '{serviceTypeName}'.");
				ServiceRuntime.RegisterServiceAsync(serviceTypeName, context => new EmailService(context)).GetAwaiter().GetResult();

				Logger.LogInformation($"{baseLogString} Main => '{serviceTypeName}' type registered.");
				ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(EmailService).Name);

				// Prevents this host process from terminating so services keep running.
				Thread.Sleep(Timeout.Infinite);
			}
			catch (Exception e)
			{
				Logger.LogError($"{baseLogString} Main => Exception: {e.Message}.", e);
				ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
				throw;
			}
		}
	}
}
