using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.NmsContracts.GDA;
using Outage.Common;

namespace OMS.Common.NmsContracts
{
	[ServiceContract]
	public interface INetworkModelGDAContract : IService
	{
		/// <summary>
		/// Updates model by appluing reosoreces sent in delta
		/// </summary>		
		/// <param name="delta">Object which contains model changes</param>		
		/// <returns>Result of model changes</returns>
		[OperationContract]
		Task<UpdateResult> ApplyUpdate(Delta delta);

		/// <summary>
		/// Gets resource description for resource specified by id.
		/// </summary>		
		/// <param name="resourceId">Resource id of the entity</param>
		/// <param name="propIds">List of requested properties</param>		
		/// <returns>Resource description of the specified entity</returns>
		[OperationContract]
		Task<ResourceDescription> GetValues(long resourceId, List<ModelCode> propIds);

		/// <summary>
		/// Gets id of the resource iterator that holds descriptions for all entities of the specified type.
		/// </summary>		
		/// <param name="entityType">Type code of entity that is requested</param>
		/// <param name="propIds">List of requested property codes</param>
		/// <returns>Id of resource iterator for the requested entities</returns>
		[OperationContract]
		Task<int> GetExtentValues(ModelCode entityType, List<ModelCode> propIds);

		/// <summary>
		/// Gets id of the resource iterator that holds descriptions for all entities related to specified source.
		/// </summary>
		/// <param name="source">Resource id of entity that is start for association search</param>
		/// <param name="propIds">List of requested property ids</param>
		/// <param name="association">Relation between source and entities that should be returned</param>		
		/// <returns>Id of the resource iterator for the requested entities</returns>
		[OperationContract]
		Task<int> GetRelatedValues(long source, List<ModelCode> propIds, Association association);

		/// <summary>
		/// Gets list of next n resource descriptions from the iterator.
		/// </summary>
		/// <param name="n">Number of next resources that should be returned</param>
		/// <param name="id">Id of the resource iterator</param>
		/// <returns>List of resource descriptions</returns>
		[OperationContract]
		Task<List<ResourceDescription>> IteratorNext(int n, int id);

		/// <summary>
		/// Resets current position in resource iterator to the iterator's beginning
		/// </summary>
		/// <param name="id">Id of the resource iterator</param>
		/// <returns>TRUE if current position in iterator is successfully reseted</returns>
		[OperationContract]
		Task<bool> IteratorRewind(int id);

		/// <summary>
		/// Gets the total number of the resource descriptions in resource iterator.
		/// </summary>
		/// <param name="id">Id of the resource iterator</param>
		/// <returns>Total number of resources in resource iterator</returns>
		[OperationContract]
		Task<int> IteratorResourcesTotal(int id);

		/// <summary>
		/// Gets the number of resource descriptions left from current position in iterator to iterator's end
		/// </summary>
		/// <param name="id">Id of the resource iterator</param>
		/// <returns>Number of resource iterator left to return in next calls</returns>
		[OperationContract]
		Task<int> IteratorResourcesLeft(int id);

		/// <summary>
		/// Closes the iterator.
		/// </summary>
		/// <param name="id">Id of the resource iterator</param>
		/// <returns>TRUE if iterator is successfully closed</returns>
		[OperationContract]
		Task<bool> IteratorClose(int id);
	}
}
