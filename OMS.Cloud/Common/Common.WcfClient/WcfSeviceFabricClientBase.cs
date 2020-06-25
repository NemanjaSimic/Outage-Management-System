using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.Cloud.Logger;
using System;
using System.Fabric;
using System.Reflection;
using System.ServiceModel;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient
{
    public class WcfSeviceFabricClientBase<TContract> : ServicePartitionClient<WcfCommunicationClient<TContract>> where TContract : class, IService
    {
        private readonly string baseLoggString;
        private readonly int maxTryCount = 30;

        private ICloudLogger logger;
        protected ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public WcfSeviceFabricClientBase(WcfCommunicationClientFactory<TContract> clientFactory, Uri serviceName, ServicePartitionKey servicePartition, string listenerName)
            : base(clientFactory, serviceName, servicePartition, TargetReplicaSelector.Default, listenerName)
        {
            this.baseLoggString = $"{typeof(WcfSeviceFabricClientBase<TContract>)} [{this.GetHashCode()}] =>";
        }

        /// <summary>
        /// Mehanizam za prevazilazenje FabricNotReadableException-a, zasnovan na re-try logici.
        /// </summary>
        /// <param name="methodName">Kratnko ime same metode (npr. Method1, a ne MojaKlasa.Method1)</param>
        /// <param name="passedParameters">Niz parametara - Mora se proslediti onoliko prametara koliko ih metoda ima. Ako metoda ima opcione paramtere mora se proslediti barem null, ako ne vrednost tih parametara.</param>
        /// <returns>Vraca objekat koji predstavlja resultat inovk-ovane metode.</returns>
        protected Task MethodWrapperAsync(string methodName, object[] passedParameters)
        {
            int objectId = this.GetHashCode();
            string debugMessage = $"{baseLoggString} MethodWrapperAsync method called => MethodName: {methodName}, ReturnType: {typeof(Task)}, passedParameters count: {passedParameters.Length}";
            Logger.LogDebug(debugMessage);

            return InvokeWithRetryAsync(client =>
            {
                string varboseMessage = $"{baseLoggString} MethodWrapperAsync => ServicePartitionClient.InvokeWithRetryAsync method called[{objectId}].";
                Logger.LogVerbose(varboseMessage);

                var type = typeof(TContract);
                var methods = type.GetMethods();

                foreach (var method in methods)
                {
                    if (methodName != method.Name)
                    {
                        continue;
                    }

                    if (method.ReturnType != typeof(Task))
                    {
                        string errMessage = $"{baseLoggString} MethodWrapperAsync => Method with name '{methodName}' has ReturnType: {method.ReturnType}, but {typeof(Task)} was expected.";
                        Logger.LogError(errMessage);
                        throw new Exception(errMessage);
                    }

                    CheckArgumentsValidity(method, passedParameters);

                    return InvokeMethodAsync(method, client.Channel, passedParameters);
                }

                string message = $"{baseLoggString} MethodWrapperAsync => {type} does not contain method with name '{methodName}'.";
                Logger.LogError(message);
                throw new Exception(message);
            });
        }

        /// <summary>
        /// Mehanizam za prevazilazenje FabricNotReadableException-a, zasnovan na re-try logici.
        /// </summary>
        /// <param name="methodName">Kratnko ime same metode (npr. Method1, a ne MojaKlasa.Method1)</param>
        /// <param name="passedParameters">Niz parametara - Mora se proslediti onoliko prametara koliko ih metoda ima. Ako metoda ima opcione paramtere mora se proslediti barem null, ako ne vrednost tih parametara.</param>
        /// <returns>Vraca objekat koji predstavlja resultat inovk-ovane metode.</returns>
        protected Task<TResult> MethodWrapperAsync<TResult>(string methodName, object[] passedParameters)
        {
            string debugMessage = $"{baseLoggString} MethodWrapperAsync<{typeof(TResult)}> method called => MethodName: {methodName}, ReturnType: {typeof(Task<TResult>)}, passedParameters count: {passedParameters.Length}";
            Logger.LogDebug(debugMessage);

            return InvokeWithRetryAsync(client =>
            {
                string varboseMessage = $"{baseLoggString} MethodWrapperAsync<{typeof(TResult)}> => InvokeWithRetryAsync method called.";
                Logger.LogVerbose(varboseMessage);
                
                var type = typeof(TContract);
                var methods = type.GetMethods();
                var passedReturenType = typeof(Task<TResult>);

                foreach (var method in methods)
                {
                    if (methodName != method.Name)
                    {
                        continue;
                    }

                    if(passedReturenType != method.ReturnType)
                    {
                        string errMessage = $"{baseLoggString} MethodWrapperAsync<{typeof(TResult)}> =>  Passed return type: {passedReturenType} does not match return type of method with name '{methodName}' [ReturnType: {method.ReturnType}].";
                        Logger.LogError(errMessage);
                        throw new Exception(errMessage);
                    }

                    CheckArgumentsValidity(method, passedParameters);

                    return InvokeMethodAsync<TResult>(method, client.Channel, passedParameters);
                }

                string message = $"{baseLoggString} MethodWrapperAsync<{typeof(TResult)}> => {type} does not contain method with name '{methodName}'.";
                Logger.LogError(message);
                throw new Exception(message);

            }, new Type[1] { typeof(CommunicationObjectFaultedException) });
        }

        protected void CheckArgumentsValidity(MethodInfo method, object[] passedParameters)
        {
            string varboseMessage = $"{baseLoggString} CheckArgumentsValidity method called => MethodName: {method.Name}, PassedParameters count: {passedParameters.Length}.";
            Logger.LogVerbose(varboseMessage);

            var paramsInfo = method.GetParameters();

            if (passedParameters.Length != paramsInfo.Length)
            {
                string errorMessage = $"{baseLoggString} CheckArgumentsValidity => Trying to invoke method {method.Name} that has {paramsInfo.Length} parameters - but passing {passedParameters.Length} parameters";
                Logger.LogError(errorMessage);
                throw new TargetParameterCountException(errorMessage);
            }

            for (int i = 0; i < paramsInfo.Length; i++)
            {
                var paramInfo = paramsInfo[i];
                var paramInfoType = paramInfo.ParameterType;

                var passedParam = passedParameters[i];

                //type not nullable and method has default value
                if (passedParam == null && (Nullable.GetUnderlyingType(paramInfoType) == null) && paramInfo.HasDefaultValue)
                {
                    passedParam = Type.Missing; //sets the mechanism that acquires the default value
                    passedParameters[i] = passedParam;
                    
                    varboseMessage = $"{baseLoggString} CheckArgumentsValidity => {Type.Missing} value assigned to non-nullable type parameter '{paramInfoType.Name}' that has default value defined.";
                    Logger.LogVerbose(varboseMessage);
                }
                //type not nullable and method does not have default value
                else if (passedParam == null && (Nullable.GetUnderlyingType(paramInfoType) == null) && !paramInfo.HasDefaultValue)
                {
                    string errorMessage = $"{baseLoggString} CheckArgumentsValidity => Trying to invoke method {method.Name} by passing null for not nullable paramter => type {paramInfoType}";
                    Logger.LogError(errorMessage);
                    throw new Exception(errorMessage);
                }

                var passedParamType = passedParam.GetType();

                if (!paramInfoType.IsAssignableFrom(passedParamType) && passedParamType != Type.Missing.GetType())
                {
                    string errorMessage = $"{baseLoggString} CheckArgumentsValidity => Trying to invoke method {method.Name} with invalid type of paramters => type {passedParamType} instead of expected {paramInfoType}";
                    Logger.LogError(errorMessage);
                    throw new Exception(errorMessage);
                }
            }

            varboseMessage = $"{baseLoggString} CheckArgumentsValidity => All passed parameters are valid.";
            Logger.LogVerbose(varboseMessage);
        }

        private async Task InvokeMethodAsync(MethodInfo method, object obj, object[] parameters)
        {
            int tryCount = 0;

            while (true)
            {
                try
                {
                    string debugMessage = $"{baseLoggString} InvokeMethodAsync => Invoking method '{method.Name}'.";
                    Logger.LogDebug(debugMessage);

                    method.Invoke(obj, parameters);
                    
                    debugMessage = $"{baseLoggString} InvokeMethodAsync => Method '{method.Name}' invoked successfully.";
                    Logger.LogDebug(debugMessage);
                }
                catch (FabricNotReadableException fnre)
                {
                    string message = $"{baseLoggString} InvokeMethodAsync => FabricNotReadableException caught while invoking {method.Name} method. RetryCount: {tryCount}";
                    Logger.LogDebug(message);

                    if (++tryCount < maxTryCount)
                    {
                        await Task.Delay(1000);
                        continue;
                    }
                    else
                    {
                        message = $"{baseLoggString} InvokeMethodAsync => FabricNotReadableException re-throwen after {maxTryCount} retries. See the inner exception for more details.";
                        Logger.LogError(message, fnre);
                        throw new Exception(message, fnre);
                    }
                }
                catch (TargetInvocationException e)
                {
                    if (e.InnerException is CommunicationObjectFaultedException communicationException)
                    {
                        string message = $"{baseLoggString} InvokeMethodAsync => CommunicationObjectFaultedException caught.";
                        Logger.LogError(message, communicationException);
                        throw communicationException;
                    }
                }
                catch (Exception e)
                {
                    string message = "{baseLoggString} InvokeMethodAsync => exception caught.";
                    Logger.LogError(message, e);
                    throw e;
                }
            }
        }

        private Task<TResult> InvokeMethodAsync<TResult>(MethodInfo method, object obj, object[] parameters)
        {
            int tryCount = 0;

            while (true)
            {
                try
                {
                    string debugMessage = $"{baseLoggString} InvokeMethodAsync<{typeof(TResult)}> => Invoking method '{method.Name}'.";
                    Logger.LogDebug(debugMessage);

                    var task = (Task<TResult>)method.Invoke(obj, parameters);

                    debugMessage = $"{baseLoggString} InvokeMethodAsync<{typeof(TResult)}> => Method '{method.Name}' invoked successfully.";
                    Logger.LogDebug(debugMessage);

                    return task;
                }
                catch (FabricNotReadableException fnre)
                {
                    string message = $"{baseLoggString} InvokeMethodAsync<{typeof(TResult)}> => FabricNotReadableException caught while invoking {method.Name} method. RetryCount: {tryCount}";
                    Logger.LogDebug(message);

                    if (++tryCount < maxTryCount)
                    {
                        Task.Delay(1000).Wait(); //Tread.Sleep(1000); ?
                        continue;
                    }
                    else
                    {
                        message = $"{baseLoggString} InvokeMethodAsync<{typeof(TResult)}> => FabricNotReadableException re-throwen after {maxTryCount} retries. See the inner exception for more details.";
                        Logger.LogError(message, fnre);
                        throw new Exception(message, fnre);
                    }
                }
                catch (TargetInvocationException e)
                {
                    if(e.InnerException is CommunicationObjectFaultedException communicationException)
                    {
                        string message = $"{baseLoggString} InvokeMethodAsync<{typeof(TResult)}> => CommunicationObjectFaultedException caught.";
                        Logger.LogError(message, communicationException);
                        throw communicationException;
                    }
                }
                catch (Exception e)
                {
                    string message = $"{baseLoggString} InvokeMethodAsync<{typeof(TResult)}> => exception caught.";
                    Logger.LogError(message, e);
                    throw e;
                }
            }
        }
    }
}
