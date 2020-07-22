
using CECommon.Model;
using System.Threading.Tasks;

namespace CECommon.Interfaces
{
    public interface IModelManager
    {
        Task<ModelDelta> TryGetAllModelEntitiesAsync();
    }
}
