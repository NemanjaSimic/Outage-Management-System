﻿using Common.CloudContracts;
using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.Cloud;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace OMS.Common.TmsContracts
{
    [ServiceContract]
    public interface ITransactionEnlistmentContract : IService, IHealthChecker
    {
        [OperationContract]
        Task<bool> Enlist(string transactionName, string transactionActorName);
    }
}
