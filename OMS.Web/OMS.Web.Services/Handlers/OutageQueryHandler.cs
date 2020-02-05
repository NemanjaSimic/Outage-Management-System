using MediatR;
using OMS.Web.Services.Queries;
using OMS.Web.UI.Models.ViewModels;
using Outage.Common;
using Outage.Common.ServiceProxies.Outage;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace OMS.Web.Services.Handlers
{
    public class OutageQueryHandler : IRequestHandler<GetActiveOutagesQuery, IEnumerable<Outage.Common.PubSub.OutageDataContract.ActiveOutage>>, IRequestHandler<GetArchivedOutagesQuery, IEnumerable<ArchivedOutage>>
    {
        private ILogger _logger;

        protected ILogger Logger
        {
            get { return _logger ?? (_logger = LoggerWrapper.Instance); }
        }

        #region Proxies

        private OutageServiceProxy _outageProxy = null;

        private OutageServiceProxy GetOutageProxy()
        {
            int numberOfTries = 0;
            int sleepInterval = 500;

            while (numberOfTries <= int.MaxValue)
            {
                try
                {
                    if (_outageProxy != null)
                    {
                        _outageProxy.Abort();
                        _outageProxy = null;
                    }

                    _outageProxy = new OutageServiceProxy(EndpointNames.OutageServiceEndpoint);
                    _outageProxy.Open();

                    if (_outageProxy.State == CommunicationState.Opened)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    string message = $"Exception on PublisherProxy initialization. Message: {ex.Message}";
                    Logger.LogWarn(message, ex);
                    _outageProxy = null;
                }
                finally
                {
                    numberOfTries++;
                    Logger.LogDebug($"FunctionExecutor: PublisherProxy getter, try number: {numberOfTries}.");

                    if (numberOfTries >= 100)
                    {
                        sleepInterval = 1000;
                    }

                    Thread.Sleep(sleepInterval);
                }
            }

            return _outageProxy;
        }

        #endregion

        public Task<IEnumerable<Outage.Common.PubSub.OutageDataContract.ActiveOutage>> Handle(GetActiveOutagesQuery request, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                using (OutageServiceProxy outageProxy = GetOutageProxy())
                {
                    try
                    {
                        IEnumerable<Outage.Common.PubSub.OutageDataContract.ActiveOutage> activeOutages = outageProxy.GetActiveOutages();
                        return activeOutages;
                    }
                    catch (Exception e)
                    {
                        Logger.LogError("Task<IEnumerable<ActiveOutage>> Handle => exception", e);
                        throw e;
                    }
                }
            });
        }

        public Task<IEnumerable<ArchivedOutage>> Handle(GetArchivedOutagesQuery request, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                using (OutageServiceProxy outageProxy = GetOutageProxy())
                {
                    IEnumerable<ArchivedOutage> archivedOutages = new List<ArchivedOutage>();
                    
                    try
                    {
                        var dbArchivedOutages = outageProxy.GetArchivedOutages();

                        foreach(var dbArchivedOutage in dbArchivedOutages)
                        {
                            ArchivedOutage archivedOutage = new ArchivedOutage()
                            {
                                Id = dbArchivedOutage.ElementGid,
                                ElementId = dbArchivedOutage.ElementGid,
                                DateCreated = dbArchivedOutage.ReportTime,
                                AfectedConsumers = new List<Consumer>(),
                            };

                            //TODO: mapper badly needed
                            foreach (var dbAfectedConsumer in dbArchivedOutage.AffectedConsumers)
                            {
                                Consumer consumer = new Consumer()
                                {
                                    ConsumerId = dbAfectedConsumer.ConsumerId,
                                    ConsumerMRID = dbAfectedConsumer.ConsumerMRID,
                                    FirstName = dbAfectedConsumer.FirstName,
                                    LastName = dbAfectedConsumer.LastName,
                                    ArchivedOutages = new List<ArchivedOutage>(),
                                };

                                archivedOutage.AfectedConsumers.Add(consumer);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogError("Task<IEnumerable<ArchivedOutage>> Handle => exception", e);
                        throw e;
                    }
                    
                    return archivedOutages;
                }
            });
        }
    }
}
