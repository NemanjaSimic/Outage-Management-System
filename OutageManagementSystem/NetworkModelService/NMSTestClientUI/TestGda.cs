using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FTN.Services.NetworkModelService.TestClientUI;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceContracts.GDA;
using Outage.Common.ServiceProxies;

namespace TelventDMS.Services.NetworkModelService.TestClient.TestsUI
{
	public sealed class TestGda : IDisposable
	{
		private ILogger logger;

		private ILogger Logger
		{
			get { return logger ?? (logger = LoggerWrapper.Instance); }
		}

		private ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
		//private ProxyFactory proxyFactory;
		readonly WcfServiceFabricCommunicationClient<INetworkModelGDAContract> wcfClient;

		public TestGda()
		{
			string uri = $"fabric:/OMS_Cloud/NMS_Stateless";

			//var binding = WcfUtility.CreateTcpClientBinding();
			//var partitionResolver = ServicePartitionResolver.GetDefault();
			//var wcfClientFactory = new WcfCommunicationClientFactory<INetworkModelGDAContract>(clientBinding: binding,
			//																				   servicePartitionResolver: partitionResolver);

			//var communicationClient = new ServicePartitionClient<WcfCommunicationClient<INetworkModelGDAContract>>(wcfClientFactory, new Uri(uri));
			//var result = communicationClient.InvokeWithRetryAsync(client => client.Channel.GetValues(1, new List<ModelCode>())).Result;
			
			//proxyFactory = new ProxyFactory();
			/*"net.tcp://localhost:10007/NetworkModelService/GDA DESKTOP-IOS3VE9:"*/
			wcfClient = WcfServiceFabricCommunicationClient<INetworkModelGDAContract>.GetClient(new Uri(uri));
		}

		#region GDAQueryService

		public async Task<ResourceDescription> GetValues(long globalId, List<ModelCode> properties)
		{
			string message = "Getting values method started.";
			Logger.LogInfo(message);

			ResourceDescription rd = null;

			try
			{
				//using (NetworkModelGDAProxy gdaQueryProxy = proxyFactory.CreateProxy<NetworkModelGDAProxy, INetworkModelGDAContract>(EndpointNames.NetworkModelGDAEndpoint))
				//{
				if (wcfClient == null)
				{
					string errMsg = "NetworkModelGDAProxy is null.";
					Logger.LogWarn(errMsg);
					throw new NullReferenceException(errMsg);
				}

				rd = wcfClient.InvokeWithRetryAsync(x => x.Channel.GetValues(globalId, properties)).Result;/*GetValues(globalId, properties).Result;*/
				message = "Getting values method successfully finished.";
				Logger.LogInfo(message);
				//}
			}
			catch (Exception e)
			{
				message = string.Format("Getting values method for entered id = {0} failed.\n\t{1}", globalId, e.Message);
				Logger.LogError(message);
			}

			return rd;
		}

		public async Task<List<long>> GetExtentValues(ModelCode modelCodeType, List<ModelCode> properties, StringBuilder sb)
		{
			string message = "Getting extent values method started.";
			Logger.LogInfo(message);

			int iteratorId;
			int resourcesLeft;
			int numberOfResources = 300;
			List<long> ids = new List<long>();
			StringBuilder tempSb = new StringBuilder();

			try
			{
				//using (NetworkModelGDAProxy gdaQueryProxy = proxyFactory.CreateProxy<NetworkModelGDAProxy, INetworkModelGDAContract>(EndpointNames.NetworkModelGDAEndpoint))
				//{
				if (wcfClient == null)
				{
					string errMsg = "NetworkModelGDAProxy is null.";
					Logger.LogWarn(errMsg);
					throw new NullReferenceException(errMsg);
				}


				iteratorId = wcfClient.InvokeWithRetryAsync(x => x.Channel.GetExtentValues(modelCodeType, properties)).Result;
				resourcesLeft = wcfClient.InvokeWithRetryAsync(x => x.Channel.IteratorResourcesLeft(iteratorId)).Result;

				while (resourcesLeft > 0)
				{
					List<ResourceDescription> rds = wcfClient.InvokeWithRetryAsync(x => x.Channel.IteratorNext(numberOfResources, iteratorId)).Result;

					for (int i = 0; i < rds.Count; i++)
					{
						if (rds[i] != null)
						{
							tempSb.Append($"Entity with gid: 0x{rds[i].Id:X16}" + Environment.NewLine);

							foreach (Property property in rds[i].Properties)
							{
								switch (property.Type)
								{
									case PropertyType.Int64:
										StringAppender.AppendLong(tempSb, property);
										break;
									case PropertyType.Float:
										StringAppender.AppendFloat(tempSb, property);
										break;
									case PropertyType.String:
										StringAppender.AppendString(tempSb, property);
										break;
									case PropertyType.Reference:
										StringAppender.AppendReference(tempSb, property);
										break;
									case PropertyType.ReferenceVector:
										StringAppender.AppendReferenceVector(tempSb, property);
										break;

									default:
										tempSb.Append($"{property.Id}: {property.PropertyValue.LongValue}{Environment.NewLine}");
										break;
								}
							}
						}
						ids.Add(rds[i].Id);
					}
					resourcesLeft = wcfClient.InvokeWithRetryAsync(x => x.Channel.IteratorResourcesLeft(iteratorId)).Result;
				}

				wcfClient.InvokeWithRetryAsync(x => x.Channel.IteratorClose(iteratorId)).Wait();

				message = "Getting extent values method successfully finished.";
				Logger.LogInfo(message);
				//}
			}
			catch (Exception e)
			{
				message = string.Format("Getting extent values method failed for {0}.\n\t{1}", modelCodeType, e.Message);
				Logger.LogError(message);
			}

			if (sb != null)
			{
				sb.Append(tempSb.ToString());
			}

			return ids;
		}

		public async Task<List<long>> GetRelatedValues(long sourceGlobalId, List<ModelCode> properties, Association association, StringBuilder sb)
		{
			string message = "Getting related values method started.";
			Logger.LogInfo(message);

			int iteratorId = 0;
			int resourcesLeft = 0;
			int numberOfResources = 500;
			List<long> resultIds = new List<long>();
			StringBuilder tempSb = new StringBuilder();

			try
			{
				//using (NetworkModelGDAProxy gdaQueryProxy = proxyFactory.CreateProxy<NetworkModelGDAProxy, INetworkModelGDAContract>(EndpointNames.NetworkModelGDAEndpoint))
				//{
				if (wcfClient == null)
				{
					string errMsg = "NetworkModelGDAProxy is null.";
					Logger.LogWarn(errMsg);
					throw new NullReferenceException(errMsg);
				}

				iteratorId = wcfClient.InvokeWithRetryAsync(x => x.Channel.GetRelatedValues(sourceGlobalId, properties, association)).Result;
				resourcesLeft = wcfClient.InvokeWithRetryAsync(x => x.Channel.IteratorResourcesLeft(iteratorId)).Result;

				while (resourcesLeft > 0)
				{
					List<ResourceDescription> rds = wcfClient.InvokeWithRetryAsync(x => x.Channel.IteratorNext(numberOfResources, iteratorId)).Result;

					for (int i = 0; i < rds.Count; i++)
					{
						if (rds[i] != null)
						{
							tempSb.Append($"Entity with gid: 0x{rds[i].Id:X16}" + Environment.NewLine);

							foreach (Property property in rds[i].Properties)
							{
								switch (property.Type)
								{
									case PropertyType.Int64:
										StringAppender.AppendLong(tempSb, property);
										break;
									case PropertyType.Float:
										StringAppender.AppendFloat(tempSb, property);
										break;
									case PropertyType.String:
										StringAppender.AppendString(tempSb, property);
										break;
									case PropertyType.Reference:
										StringAppender.AppendReference(tempSb, property);
										break;
									case PropertyType.ReferenceVector:
										StringAppender.AppendReferenceVector(tempSb, property);
										break;

									default:
										tempSb.Append($"{property.Id}: {property.PropertyValue.LongValue}{Environment.NewLine}");
										break;
								}
							}
						}
						resultIds.Add(rds[i].Id);
					}
					resourcesLeft = wcfClient.InvokeWithRetryAsync(x => x.Channel.IteratorResourcesLeft(iteratorId)).Result;
				}
				wcfClient.InvokeWithRetryAsync(x => x.Channel.IteratorClose(iteratorId)).Wait();

				message = "Getting related values method successfully finished.";
				Logger.LogInfo(message);
				//}
			}
			catch (Exception e)
			{
				message = string.Format("Getting related values method  failed for sourceGlobalId = {0} and association (propertyId = {1}, type = {2}). Reason: {3}", sourceGlobalId, association.PropertyId, association.Type, e.Message);
				Logger.LogError(message);
			}

			if (sb != null)
			{
				sb.Append(tempSb.ToString());
			}

			return resultIds;
		}

		#endregion GDAQueryService

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}
	}
}
