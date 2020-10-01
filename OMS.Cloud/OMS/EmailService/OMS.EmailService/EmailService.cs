using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.EmailImplementation.Interfaces;
using OMS.EmailImplementation.Factories;
using OMS.Common.Cloud.Logger;
using System;

namespace OMS.EmailService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class EmailService : StatelessService
	{
		private readonly string baseLogString;

		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

		public EmailService(StatelessServiceContext context)
			: base(context)
		{
            this.logger = CloudLoggerFactory.GetLogger(ServiceEventSource.Current, context);

            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
			string verboseMessage = $"{baseLogString} entering Ctor.";
			Logger.LogVerbose(verboseMessage);
		}

		

		/// <summary>
		/// This is the main entry point for your service instance.
		/// </summary>
		/// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
		//protected override async Task RunAsync(CancellationToken cancellationToken)
		//{
  //          //modo: neki while dok se ne konektuje

  //          try
  //          {
  //              IIdleEmailClient idleEmailclient = new ImapIdleClientFactory().CreateClient();

  //              if (!idleEmailclient.Connect())
  //              {
  //                  Logger.LogError($"{baseLogString} RunAsync => idleEmailclient.Connect() returned false.");
  //                  return;
  //              }

  //              idleEmailclient.RegisterIdleHandler();

  //              if (!idleEmailclient.StartIdling())
  //              {
  //                  Logger.LogError($"{baseLogString} RunAsync => idleEmailclient.StartIdling() returned false.");
  //                  return;
  //              }
  //          }
  //          catch (Exception e)
  //          {
  //              Logger.LogError($"{baseLogString} RunAsync => Exception: {e.Message}");
  //          }
  //      }
	}
}
