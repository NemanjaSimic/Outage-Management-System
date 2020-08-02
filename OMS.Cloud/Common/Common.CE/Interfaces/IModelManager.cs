using System.Threading.Tasks;

namespace Common.CE.Interfaces
{
	public interface IModelManager
    {
        Task<IModelDelta> TryGetAllModelEntitiesAsync();
    }
}
