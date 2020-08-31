using Common.CloudContracts;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.Cloud.Logger;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Reflection;
using System.ServiceModel;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient
{
    public class WcfSeviceFabricClientBase<TContract> : ServicePartitionClient<WcfCommunicationClient<TContract>> where TContract : class, IService, IHealthChecker
    {
        private const int maxTryCount = 30;
        private readonly string baseLogString;

        #region Private Properties
        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
        #endregion Private Properties

        public WcfSeviceFabricClientBase(WcfCommunicationClientFactory<TContract> clientFactory, Uri serviceName, ServicePartitionKey servicePartition, string listenerName)
            : base(clientFactory, serviceName, servicePartition, TargetReplicaSelector.Default, listenerName)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
        }

        /// <summary>
        /// Mehanizam za prevazilazenje FabricNotReadableException-a, zasnovan na re-try logici.
        /// </summary>
        /// <param name="methodName">Kratnko ime same metode (npr. Method1, a ne MojaKlasa.Method1)</param>
        /// <param name="passedParameters">Niz parametara - Mora se proslediti onoliko prametara koliko ih metoda ima. Ako metoda ima opcione paramtere mora se proslediti barem null, ako ne vrednost tih parametara.</param>
        /// <returns>Vraca objekat koji predstavlja resultat inovk-ovane metode.</returns>
        protected async Task MethodWrapperAsync(string methodName, object[] passedParameters)
        {
            int objectId = this.GetHashCode();
            string debugMessage = $"{baseLogString} MethodWrapperAsync method called => MethodName: {methodName}, ReturnType: {typeof(Task)}, passedParameters count: {passedParameters.Length}";
            Logger.LogDebug(debugMessage);

            try
            {
                await InvokeWithRetryAsync(client =>
                {
                    try
                    {
                        string varboseMessage = $"{baseLogString} MethodWrapperAsync => ServicePartitionClient.InvokeWithRetryAsync method called[{objectId}].";
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
                                string errMessage = $"{baseLogString} MethodWrapperAsync => Method with name '{methodName}' has ReturnType: {method.ReturnType}, but {typeof(Task)} was expected.";
                                Logger.LogError(errMessage);
                                throw new Exception(errMessage);
                            }

                            CheckArgumentsValidity(method, passedParameters);

                            return InvokeMethodAsync(method, client.Channel, passedParameters);
                        }

                        string message = $"{baseLogString} MethodWrapperAsync => {type} does not contain method with name '{methodName}'.";
                        Logger.LogError(message);
                        throw new Exception(message);
                    }
                    catch (OperationCanceledException e)
                    {
                        string message = $"{baseLogString} MethodWrapperAsync => Exception ({e.GetType()}).";
                        Logger.LogError(message, e);
                        throw;
                    }
                    catch (CommunicationObjectFaultedException e)
                    {
                        string message = $"{baseLogString} MethodWrapperAsync => Exception ({e.GetType()}).";
                        Logger.LogError(message, e);
                        throw;
                    }
                    catch (Exception e)
                    {
                        string message = $"{baseLogString} MethodWrapperAsync => Exception ({e.GetType()}).";
                        Logger.LogError(message);
                        throw;
                    }
                }, new List<Type>() { typeof(CommunicationObjectFaultedException) }.ToArray());
            }
            catch (OperationCanceledException e)
            {
                string message = $"{baseLogString} MethodWrapperAsync => Exception ({e.GetType()}).";
                Logger.LogError(message, e);
                throw;
            }
            catch (CommunicationObjectFaultedException e)
            {
                string message = $"{baseLogString} MethodWrapperAsync => Exception ({e.GetType()}).";
                Logger.LogError(message, e);
                throw;
            }
            catch (Exception e)
            {
                string message = $"{baseLogString} MethodWrapperAsync => Exception ({e.GetType()}).";
                Logger.LogError(message);
                throw;
            }
        }

        /// <summary>
        /// Mehanizam za prevazilazenje FabricNotReadableException-a, zasnovan na re-try logici.
        /// </summary>
        /// <param name="methodName">Kratnko ime same metode (npr. Method1, a ne MojaKlasa.Method1)</param>
        /// <param name="passedParameters">Niz parametara - Mora se proslediti onoliko prametara koliko ih metoda ima. Ako metoda ima opcione paramtere mora se proslediti barem null, ako ne vrednost tih parametara.</param>
        /// <returns>Vraca objekat koji predstavlja resultat inovk-ovane metode.</returns>
        protected async Task<TResult> MethodWrapperAsync<TResult>(string methodName, object[] passedParameters)
        {
            string debugMessage = $"{baseLogString} MethodWrapperAsync<{typeof(TResult)}> method called => MethodName: {methodName}, ReturnType: {typeof(Task<TResult>)}, passedParameters count: {passedParameters.Length}";
            Logger.LogDebug(debugMessage);

            TResult resut;

            try
            {
                resut = await InvokeWithRetryAsync(client =>
                {
                    try
                    {
                        string varboseMessage = $"{baseLogString} MethodWrapperAsync<{typeof(TResult)}> => InvokeWithRetryAsync method called.";
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

                            if (passedReturenType != method.ReturnType)
                            {
                                string errMessage = $"{baseLogString} MethodWrapperAsync<{typeof(TResult)}> =>  Passed return type: {passedReturenType} does not match return type of method with name '{methodName}' [ReturnType: {method.ReturnType}].";
                                Logger.LogError(errMessage);
                                throw new Exception(errMessage);
                            }

                            CheckArgumentsValidity(method, passedParameters);

                            return InvokeMethodAsync<TResult>(method, client.Channel, passedParameters);
                        }

                        string message = $"{baseLogString} MethodWrapperAsync<{typeof(TResult)}> => {type} does not contain method with name '{methodName}'.";
                        Logger.LogError(message);
                        throw new Exception(message);
                    }
                    catch (OperationCanceledException e)
                    {
                        string message = $"{baseLogString} MethodWrapperAsync<{typeof(TResult)}> => Exception ({e.GetType()}).";
                        Logger.LogError(message, e);
                        throw;
                    }
                    catch (CommunicationObjectFaultedException e)
                    {
                        string message = $"{baseLogString} MethodWrapperAsync<{typeof(TResult)}> => Exception ({e.GetType()}).";
                        Logger.LogError(message, e);
                        throw;
                    }
                    catch (Exception e)
                    {
                        string message = $"{baseLogString} MethodWrapperAsync<{typeof(TResult)}> => Exception ({e.GetType()}).";
                        Logger.LogError(message);
                        throw;
                    }
                }, new List<Type>() { typeof(CommunicationObjectFaultedException) }.ToArray());
            }
            catch (OperationCanceledException e)
            {
                string message = $"{baseLogString} MethodWrapperAsync<{typeof(TResult)}> => Exception ({e.GetType()}).";
                Logger.LogError(message, e);
                throw;
            }
            catch (CommunicationObjectFaultedException e)
            {
                string message = $"{baseLogString} MethodWrapperAsync<{typeof(TResult)}> => Exception ({e.GetType()}).";
                Logger.LogError(message, e);
                throw;
            }
            catch (Exception e)
            {
                string message = $"{baseLogString} MethodWrapperAsync<{typeof(TResult)}> => Exception ({e.GetType()}).";
                Logger.LogError(message);
                throw;
            }

            return resut;
        }

        protected void CheckArgumentsValidity(MethodInfo method, object[] passedParameters)
        {
            string varboseMessage = $"{baseLogString} CheckArgumentsValidity method called => MethodName: {method.Name}, PassedParameters count: {passedParameters.Length}.";
            Logger.LogVerbose(varboseMessage);

            var paramsInfo = method.GetParameters();

            if (passedParameters.Length != paramsInfo.Length)
            {
                string errorMessage = $"{baseLogString} CheckArgumentsValidity => Trying to invoke method {method.Name} that has {paramsInfo.Length} parameters - but passing {passedParameters.Length} parameters";
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
                    
                    varboseMessage = $"{baseLogString} CheckArgumentsValidity => {Type.Missing} value assigned to non-nullable type parameter '{paramInfoType.Name}' that has default value defined.";
                    Logger.LogVerbose(varboseMessage);
                }
                //type not nullable and method does not have default value
                else if (passedParam == null && (Nullable.GetUnderlyingType(paramInfoType) == null) && !paramInfo.HasDefaultValue)
                {
                    string errorMessage = $"{baseLogString} CheckArgumentsValidity => Trying to invoke method {method.Name} by passing null for not nullable paramter => type {paramInfoType}";
                    Logger.LogError(errorMessage);
                    throw new Exception(errorMessage);
                }

                var passedParamType = passedParam.GetType();

                if (!paramInfoType.IsAssignableFrom(passedParamType) && passedParamType != Type.Missing.GetType())
                {
                    string errorMessage = $"{baseLogString} CheckArgumentsValidity => Trying to invoke method {method.Name} with invalid type of paramters => type {passedParamType} instead of expected {paramInfoType}";
                    Logger.LogError(errorMessage);
                    throw new Exception(errorMessage);
                }
            }

            varboseMessage = $"{baseLogString} CheckArgumentsValidity => All passed parameters are valid.";
            Logger.LogVerbose(varboseMessage);
        }

        private async Task InvokeMethodAsync(MethodInfo method, object obj, object[] parameters)
        {
            int tryCount = 0;

            while (true)
            {
                try
                {
                    string debugMessage = $"{baseLogString} InvokeMethodAsync => Invoking method '{method.Name}'.";
                    Logger.LogDebug(debugMessage);

                    method.Invoke(obj, parameters);
                    
                    debugMessage = $"{baseLogString} InvokeMethodAsync => Method '{method.Name}' invoked successfully.";
                    Logger.LogDebug(debugMessage);
                    return;
                }
                catch (FabricNotReadableException fnre)
                {
                    string message = $"{baseLogString} InvokeMethodAsync => FabricNotReadableException caught while invoking {method.Name} method. RetryCount: {tryCount}";
                    Logger.LogDebug(message);

                    if (++tryCount < maxTryCount)
                    {
                        await Task.Delay(1000);
                        continue;
                    }
                    else
                    {
                        message = $"{baseLogString} InvokeMethodAsync => FabricNotReadableException re-throwen after {maxTryCount} retries. See the inner exception for more details.";
                        Logger.LogError(message, fnre);
                        throw new Exception(message, fnre);
                    }
                }
                catch (TargetInvocationException e)
                {
                    if (e.InnerException is CommunicationObjectFaultedException communicationException)
                    {
                        string message = $"{baseLogString} InvokeMethodAsync => CommunicationObjectFaultedException caught.";
                        Logger.LogError(message, communicationException);
                        throw communicationException;
                    }
                }
                catch (Exception e)
                {
                    string message = "{baseLogString} InvokeMethodAsync => exception caught.";
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
                    string debugMessage = $"{baseLogString} InvokeMethodAsync<{typeof(TResult)}> => Invoking method '{method.Name}'.";
                    Logger.LogDebug(debugMessage);

                    var task = (Task<TResult>)method.Invoke(obj, parameters);

                    debugMessage = $"{baseLogString} InvokeMethodAsync<{typeof(TResult)}> => Method '{method.Name}' invoked successfully.";
                    Logger.LogDebug(debugMessage);

                    return task;
                }
                catch (FabricNotReadableException fnre)
                {
                    string message = $"{baseLogString} InvokeMethodAsync<{typeof(TResult)}> => FabricNotReadableException caught while invoking {method.Name} method. RetryCount: {tryCount}";
                    Logger.LogDebug(message);

                    if (++tryCount < maxTryCount)
                    {
                        Task.Delay(1000).Wait(); //Tread.Sleep(1000);
                        continue;
                    }
                    else
                    {
                        message = $"{baseLogString} InvokeMethodAsync<{typeof(TResult)}> => FabricNotReadableException re-throwen after {maxTryCount} retries. See the inner exception for more details.";
                        Logger.LogError(message, fnre);
                        throw new Exception(message, fnre);
                    }
                }
                catch (TargetInvocationException e)
                {
                    if (e.InnerException is CommunicationObjectFaultedException communicationException)
                    {
                        string message = $"{baseLogString} InvokeMethodAsync<{typeof(TResult)}> => CommunicationObjectFaultedException caught.";
                        Logger.LogError(message, communicationException);
                        throw communicationException;
                    }
                }
                catch (Exception e)
                {
                    string message = $"{baseLogString} InvokeMethodAsync<{typeof(TResult)}> => exception caught.";
                    Logger.LogError(message, e);
                    throw e;
                }
            }
        }
    }
}
