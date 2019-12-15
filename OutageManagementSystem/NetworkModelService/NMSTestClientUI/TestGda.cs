using System;
using System.Collections.Generic;
using System.Text;
using FTN.Services.NetworkModelService.TestClientUI;
using Outage.Common;
using Outage.ServiceContracts;
using Outage.Common.GDA;

namespace TelventDMS.Services.NetworkModelService.TestClient.TestsUI
{
    public class TestGda : IDisposable
	{			

		private ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();

		private NetworkModelGDAProxy gdaQueryProxy = null;
		private NetworkModelGDAProxy GdaQueryProxy
		{
			get
			{
				if (gdaQueryProxy != null)
				{
					gdaQueryProxy.Abort();
					gdaQueryProxy = null;
				}

				gdaQueryProxy = new NetworkModelGDAProxy("NetworkModelGDAEndpoint");
				gdaQueryProxy.Open();

				return gdaQueryProxy;
			}
		}
		
		public TestGda()
		{ 
		}

		#region GDAQueryService

		public ResourceDescription GetValues(long globalId, List<ModelCode> properties)
		{
			string message = "Getting values method started.";
			//CommonTrace.WriteTrace(CommonTrace.TraceError, message);
            LoggerWrapper.Instance.LogInfo(message);

			ResourceDescription rd = null;
						
			try
			{
				rd = GdaQueryProxy.GetValues(globalId, properties);

				message = "Getting values method successfully finished.";
				CommonTrace.WriteTrace(CommonTrace.TraceError, message);
			}
			catch (Exception e)
			{
				message = string.Format("Getting values method for entered id = {0} failed.\n\t{1}", globalId, e.Message);
				CommonTrace.WriteTrace(CommonTrace.TraceError, message);
			}

			return rd;
		}

		public List<long> GetExtentValues(ModelCode modelCodeType, List<ModelCode> properties, StringBuilder sb)
		{
			string message = "Getting extent values method started.";
			CommonTrace.WriteTrace(CommonTrace.TraceError, message);

			int iteratorId = 0;
			int resourcesLeft = 0;
			int numberOfResources = 300;
			List<long> ids = new List<long>();
			StringBuilder tempSb = new StringBuilder();

			try
			{
				iteratorId = GdaQueryProxy.GetExtentValues(modelCodeType, properties);
				resourcesLeft = GdaQueryProxy.IteratorResourcesLeft(iteratorId);

				while (resourcesLeft > 0)
				{
					List<ResourceDescription> rds = GdaQueryProxy.IteratorNext(numberOfResources, iteratorId);

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
					resourcesLeft = GdaQueryProxy.IteratorResourcesLeft(iteratorId);
				}
				GdaQueryProxy.IteratorClose(iteratorId);

				message = "Getting extent values method successfully finished.";
				CommonTrace.WriteTrace(CommonTrace.TraceError, message);
			}			
			catch (Exception e)
			{
				message = string.Format("Getting extent values method failed for {0}.\n\t{1}", modelCodeType, e.Message);
				CommonTrace.WriteTrace(CommonTrace.TraceError, message);
			}
			
			if(sb != null)
			{
				sb.Append(tempSb.ToString());
			}

			return ids;
		}

		public List<long> GetRelatedValues(long sourceGlobalId, List<ModelCode> properties, Association association, StringBuilder sb)
		{
			string message = "Getting related values method started.";
			CommonTrace.WriteTrace(CommonTrace.TraceError, message);

			int iteratorId = 0;
			int resourcesLeft = 0;
			int numberOfResources = 500;
			List<long> resultIds = new List<long>();
			StringBuilder tempSb = new StringBuilder();

			try
			{
				iteratorId = GdaQueryProxy.GetRelatedValues(sourceGlobalId, properties, association);
				resourcesLeft = GdaQueryProxy.IteratorResourcesLeft(iteratorId);

				while (resourcesLeft > 0)
				{
					List<ResourceDescription> rds = GdaQueryProxy.IteratorNext(numberOfResources, iteratorId);

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
					resourcesLeft = GdaQueryProxy.IteratorResourcesLeft(iteratorId);
				}
				GdaQueryProxy.IteratorClose(iteratorId);

				message = "Getting related values method successfully finished.";
				CommonTrace.WriteTrace(CommonTrace.TraceError, message);				
			}
			catch (Exception e)
			{
				message = string.Format("Getting related values method  failed for sourceGlobalId = {0} and association (propertyId = {1}, type = {2}). Reason: {3}", sourceGlobalId, association.PropertyId, association.Type, e.Message);
				CommonTrace.WriteTrace(CommonTrace.TraceError, message);
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
