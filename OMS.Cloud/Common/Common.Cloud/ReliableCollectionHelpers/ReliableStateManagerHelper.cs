using Microsoft.ServiceFabric.Data;
using System;
using System.Fabric;
using System.Threading.Tasks;

namespace OMS.Common.Cloud.ReliableCollectionHelpers
{
    public class ReliableStateManagerHelper
    {
        private readonly int maxTryCount = 60;

        public async Task<T> GetOrAddAsync<T>(IReliableStateManager stateManager, ITransaction tx, string name) where T : IReliableState 
        {
            int tryCount = 0;

            while (true)
            {
                try
                {
                    return await stateManager.GetOrAddAsync<T>(tx, name);
                }
                catch (FabricNotReadableException fnre)
                {
                    if (++tryCount < maxTryCount)
                    {
                        await Task.Delay(1000);
                        //TOOD: log
                        continue;
                    }
                    else
                    {
                        string message = $"FabricNotReadableException re-throwen after {maxTryCount} retries. See the inner exception for more details.";
                        throw new Exception(message, fnre);
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public async Task<ConditionalValue<T>> TryGetAsync<T>(IReliableStateManager stateManager, string name) where T : IReliableState
        {
            int tryCount = 0;

            while (true)
            {
                try
                {
                    return await stateManager.TryGetAsync<T>(name);
                }
                catch (FabricNotReadableException fnre)
                {
                    if (++tryCount < maxTryCount)
                    {
                        await Task.Delay(1000);
                        //TOOD: log
                        continue;
                    }
                    else
                    {
                        throw fnre;
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
    }
}
