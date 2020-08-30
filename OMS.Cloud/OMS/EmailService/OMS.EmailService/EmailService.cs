using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.EmailImplementation.Interfaces;
using OMS.EmailImplementation.Factories;

namespace OMS.EmailService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class EmailService : StatelessService
	{
		public EmailService(StatelessServiceContext context)
			: base(context)
		{
		
		}

		

		/// <summary>
		/// This is the main entry point for your service instance.
		/// </summary>
		/// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
		protected override async Task RunAsync(CancellationToken cancellationToken)
		{
			

			IIdleEmailClient idleEmailclient = new ImapIdleClientFactory().CreateClient();

			if (!idleEmailclient.Connect())
			{
				//TODO log error
				return;
			}

			idleEmailclient.RegisterIdleHandler();

			if (!idleEmailclient.StartIdling())
			{
				//TODO log error
			}

			//await Task.Delay(-1); //TODO: for now
		}
	}
}
