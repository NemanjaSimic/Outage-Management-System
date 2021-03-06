﻿using System;
using System.ServiceModel;
using System.Threading;

namespace Outage.Common.ServiceProxies
{
    public class ProxyFactory : IProxyFactory
    {
        private ILogger logger;

        private ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        public TProxy CreateProxy<TProxy, TChannel>(string endpointName) where TChannel : class
                                                                         where TProxy : BaseProxy<TChannel>
        {
            TProxy proxy = null;

            int numberOfTries = 1;
            int sleepInterval = 500;

            while (numberOfTries <= int.MaxValue)
            {
                try 
                {
                    proxy = (TProxy) Activator.CreateInstance(typeof(TProxy), new object[] { endpointName });
                    proxy.Open();

                    if (proxy.State == CommunicationState.Opened)
                    {
                        //SUCCESS
                        Logger.LogDebug($"CreateProxy => {typeof(TProxy)} SUCCESSFULL get [number of tries: {numberOfTries}].");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    string message = $"Exception on {typeof(TProxy)} initialization. Message: {ex.Message}";
                    Logger.LogWarn(message, ex);

                    numberOfTries++;
                    Logger.LogDebug($"NetworkModel: {typeof(TProxy)} getter, try number: {numberOfTries}.");

                    if (numberOfTries >= 100)
                    {
                        sleepInterval = 1000;
                    }

                    Thread.Sleep(sleepInterval);
                }
            }

            if(proxy == null)
            {
                string message = $"{typeof(TProxy)} proxy is null. This can be due to the maximum number of retries is reached.";
                Logger.LogWarn(message);
                throw new NullReferenceException(message);
            }

            return proxy;
        }
    
        public TProxy CreateProxy<TProxy, TChannel>(object callback, string endpointName) where TChannel : class
                                                                                          where TProxy : DuplexClientBase<TChannel>
        {
            TProxy proxy = null;

            int numberOfTries = 1;
            int sleepInterval = 500;

            while (numberOfTries <= int.MaxValue)
            {
                try
                {
                    proxy = (TProxy)Activator.CreateInstance(typeof(TProxy), new object[] { callback, endpointName });
                    proxy.Open();

                    if (proxy.State == CommunicationState.Opened)
                    {
                        //SUCCESS
                        Logger.LogDebug($"CreateProxy => {typeof(TProxy)} SUCCESSFULL get [number of tries: {numberOfTries}].");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    string message = $"Exception on {typeof(TProxy)} initialization. Message: {ex.Message}";
                    Logger.LogWarn(message, ex);

                    numberOfTries++;
                    Logger.LogDebug($"NetworkModel: {typeof(TProxy)} getter, try number: {numberOfTries}.");

                    if (numberOfTries >= 100)
                    {
                        sleepInterval = 1000;
                    }

                    Thread.Sleep(sleepInterval);
                }
            }

            if (proxy == null)
            {
                string message = $"{typeof(TProxy)} proxy is null. This can be due to the maximum number of retries is reached.";
                Logger.LogWarn(message);
                throw new NullReferenceException(message);
            }

            return proxy;
        }
    }
}
