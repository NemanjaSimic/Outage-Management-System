using Microsoft.ServiceFabric.Data;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Common.Cloud
{
    public class ReliableStateManagerHelper
    {
        private readonly int maxTryCount = 30;

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
                    if(++tryCount < maxTryCount)
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
                catch (Exception)
                {
                    throw;
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
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}
