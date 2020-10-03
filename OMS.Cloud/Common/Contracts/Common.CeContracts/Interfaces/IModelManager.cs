using System.Threading.Tasks;

namespace Common.CeContracts
{ 

    public interface IModelManager
    {
        Task<ModelDelta> TryGetAllModelEntitiesAsync();
    }
}
