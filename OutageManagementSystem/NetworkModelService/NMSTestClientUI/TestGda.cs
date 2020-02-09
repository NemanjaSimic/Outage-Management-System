using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using System.Threading;
using FTN.Services.NetworkModelService.TestClientUI;
using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceContracts.GDA;
using Outage.Common.ServiceProxies;

namespace TelventDMS.Services.NetworkModelService.TestClient.TestsUI
{
	public class TestGda : IDisposable
	{
		private ILogger logger;

		protected ILogger Logger
		{
			get { return logger ?? (logger = LoggerWrapper.Instance); }
		}

		private ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
		private ProxyFactory proxyFactory;

		//      #region Proxies
		//      private NetworkModelGDAProxy gdaQueryProxy = null;

		//private NetworkModelGDAProxy GetGdaQueryProxy()
		//{
		//          int numberOfTries = 0;
		//	int sleepInterval = 500;

		//	while (numberOfTries <= int.MaxValue)
		//          {
		//              try
		//		{
		//			if (gdaQueryProxy != null)
		//			{
		//				gdaQueryProxy.Abort();
		//				gdaQueryProxy = null;
		//			}

		//			gdaQueryProxy = new NetworkModelGDAProxy(EndpointNames.NetworkModelGDAEndpoint);
		//			gdaQueryProxy.Open();

		//			if (gdaQueryProxy.State == CommunicationState.Opened)
		//			{
		//				break;
		//			}
		//		}
		//		catch (Exception ex)
		//		{
		//			string message = $"Exception on NetworkModelGDAProxy initialization. Message: {ex.Message}";
		//			Logger.LogWarn(message, ex);
		//			gdaQueryProxy = null;
		//		}
		//		finally
		//              {
		//                  numberOfTries++;
		//                  Logger.LogDebug($"TestGda: NetworkModelGDAProxy getter, try number: {numberOfTries}.");

		//			if (numberOfTries >= 100)
		//			{
		//				sleepInterval = 1000;
		//			}

		//			Thread.Sleep(sleepInterval);
		//              }
		//          }

		//	return gdaQueryProxy;
		//}

		//#endregion

		public TestGda()
		{
			proxyFactory = new ProxyFactory();
		}

		#region GDAQueryService

		public ResourceDescription GetValues(long globalId, List<ModelCode> properties)
		{
			string message = "Getting values method started.";
			Logger.LogInfo(message);

			ResourceDescription rd = null;

			try
			{
				using (NetworkModelGDAProxy gdaQueryProxy = proxyFactory.CreateProxy<NetworkModelGDAProxy, INetworkModelGDAContract>(EndpointNames.NetworkModelGDAEndpoint))
				{
					if (gdaQueryProxy != null)
					{
						rd = gdaQueryProxy.GetValues(globalId, properties);
						message = "Getting values method successfully finished.";
						Logger.LogInfo(message);
					}
					else
					{
						string errMsg = "NetworkModelGDAProxy is null.";
						Logger.LogWarn(errMsg);
						throw new NullReferenceException(errMsg);
					}
				}
			}
			catch (Exception e)
			{
				message = string.Format("Getting values method for entered id = {0} failed.\n\t{1}", globalId, e.Message);
				Logger.LogError(message);
			}

			return rd;
		}

		public List<long> GetExtentValues(ModelCode modelCodeType, List<ModelCode> properties, StringBuilder sb)
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
				using (NetworkModelGDAProxy gdaQueryProxy = proxyFactory.CreateProxy<NetworkModelGDAProxy, INetworkModelGDAContract>(EndpointNames.NetworkModelGDAEndpoint))
				{
					if (gdaQueryProxy == null)
					{
						string errMsg = "NetworkModelGDAProxy is null.";
						Logger.LogWarn(errMsg);
						throw new NullReferenceException(errMsg);
					}

					iteratorId = gdaQueryProxy.GetExtentValues(modelCodeType, properties);
					resourcesLeft = gdaQueryProxy.IteratorResourcesLeft(iteratorId);

					while (resourcesLeft > 0)
					{
						List<ResourceDescription> rds = gdaQueryProxy.IteratorNext(numberOfResources, iteratorId);

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
						resourcesLeft = gdaQueryProxy.IteratorResourcesLeft(iteratorId);
					}

					gdaQueryProxy.IteratorClose(iteratorId);

					message = "Getting extent values method successfully finished.";
					Logger.LogInfo(message);
				}
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

		public List<long> GetRelatedValues(long sourceGlobalId, List<ModelCode> properties, Association association, StringBuilder sb)
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
				using (NetworkModelGDAProxy gdaQueryProxy = proxyFactory.CreateProxy<NetworkModelGDAProxy, INetworkModelGDAContract>(EndpointNames.NetworkModelGDAEndpoint))
				{
					if (gdaQueryProxy != null)
					{
						iteratorId = gdaQueryProxy.GetRelatedValues(sourceGlobalId, properties, association);
						resourcesLeft = gdaQueryProxy.IteratorResourcesLeft(iteratorId);

						while (resourcesLeft > 0)
						{
							List<ResourceDescription> rds = gdaQueryProxy.IteratorNext(numberOfResources, iteratorId);

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
							resourcesLeft = gdaQueryProxy.IteratorResourcesLeft(iteratorId);
						}
						gdaQueryProxy.IteratorClose(iteratorId);

						message = "Getting related values method successfully finished.";
						Logger.LogInfo(message);
					}
					else
					{
						string errMsg = "NetworkModelGDAProxy is null.";
						Logger.LogWarn(errMsg);
						throw new NullReferenceException(errMsg);
					}
				}
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
