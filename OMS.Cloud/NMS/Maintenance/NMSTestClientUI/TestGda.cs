using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FTN.Services.NetworkModelService.TestClientUI;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.NmsContracts;
using OMS.Common.NmsContracts.GDA;
using OMS.Common.WcfClient.NMS;


namespace TelventDMS.Services.NetworkModelService.TestClient.TestsUI
{
    public sealed class TestGda : IDisposable
	{
		#region Private Properties
		private ICloudLogger logger;

		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}
		#endregion Private Properties

		#region GDAQueryService
		public async Task<ResourceDescription> GetValues(long globalId, List<ModelCode> properties)
		{
			string message = "Getting values method started.";
			Logger.LogInformation(message);

			ResourceDescription rd = null;

			try
			{
				var nmsClient = NetworkModelGdaClient.CreateClient();

				if (nmsClient == null)
				{
					string errMsg = "NetworkModelGdaClient is null.";
					Logger.LogWarning(errMsg);
					throw new NullReferenceException(errMsg);
				}

				rd = await nmsClient.GetValues(globalId, properties);
				message = "Getting values method SUCCESSFULLY finished.";
				Logger.LogInformation(message);
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
			Logger.LogInformation(message);

			int iteratorId;
			int resourcesLeft;
			int numberOfResources = 300;
			List<long> ids = new List<long>();
			StringBuilder tempSb = new StringBuilder();

			try
			{
				var nmsClient = NetworkModelGdaClient.CreateClient();

				if (nmsClient == null)
				{
					string errMsg = "NetworkModelGdaClient is null.";
					Logger.LogWarning(errMsg);
					throw new NullReferenceException(errMsg);
				}

				iteratorId = await nmsClient.GetExtentValues(modelCodeType, properties);
				resourcesLeft = await nmsClient.IteratorResourcesLeft(iteratorId);

				while (resourcesLeft > 0)
				{
					List<ResourceDescription> rds = await nmsClient.IteratorNext(numberOfResources, iteratorId);

					for (int i = 0; i < rds.Count; i++)
					{
						if (rds[i] == null)
						{
							continue;
						}

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

						ids.Add(rds[i].Id);
					}

					resourcesLeft = await nmsClient.IteratorResourcesLeft(iteratorId);
				}

				await nmsClient.IteratorClose(iteratorId);

				message = "Getting extent values method SUCCESSFULLY finished.";
				Logger.LogInformation(message);
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
			Logger.LogInformation(message);

			int iteratorId = 0;
			int resourcesLeft = 0;
			int numberOfResources = 500;
			List<long> resultIds = new List<long>();
			StringBuilder tempSb = new StringBuilder();

			try
			{
				var nmsClient = NetworkModelGdaClient.CreateClient();

				if (nmsClient == null)
				{
					string errMsg = "NetworkModelGdaClient is null.";
					Logger.LogWarning(errMsg);
					throw new NullReferenceException(errMsg);
				}

				iteratorId = await nmsClient.GetRelatedValues(sourceGlobalId, properties, association);
				resourcesLeft = await nmsClient.IteratorResourcesLeft(iteratorId);

				while (resourcesLeft > 0)
				{
					List<ResourceDescription> rds = await nmsClient.IteratorNext(numberOfResources, iteratorId);

					for (int i = 0; i < rds.Count; i++)
					{
						if (rds[i] == null)
						{
							continue;
						}

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

						resultIds.Add(rds[i].Id);
					}

					resourcesLeft = await nmsClient.IteratorResourcesLeft(iteratorId);
				}

				await nmsClient.IteratorClose(iteratorId);

				message = "Getting related values method SUCCESSFULLY finished.";
				Logger.LogInformation(message);
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
