using Outage.Common;
using Outage.Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace NetworkModelServiceFunctions
{
	class ModelHelper
	{
		//privremeno
		private static NetTcpBinding binding = new NetTcpBinding();
		private static EndpointAddress address = new EndpointAddress(" ");

		private ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
		private NetworkModelGDA networkModelGDA = new NetworkModelGDA(binding, address);


		public Dictionary<ModelCode, List<long>> GetAllModelEntities()
		{
			List<ModelCode> concreteClasses = modelResourcesDesc.NonAbstractClassIds;
			Dictionary<ModelCode, List<long>> modelEntities = new Dictionary<ModelCode, List<long>>();

			foreach (var concreteClass in concreteClasses)
			{
				modelEntities.Add(concreteClass, GetAllGids(concreteClass));
			}

			return modelEntities;
		}


		public List<long> GetAllGids(ModelCode concreteClass)
		{
			List<long> gids = new List<long>();

			List<ResourceDescription> resourceDescriptions = networkModelGDA.GetExtentValues(concreteClass, new List<ModelCode>() { ModelCode.IDOBJ_GID });
			foreach (var resourceDescription in resourceDescriptions)
			{
				gids.Add(resourceDescription.GetProperty(ModelCode.IDOBJ_GID).AsLong());
			}

			return gids;
		}
	}
}
