using OMSCommon.Mappers;
using OMSCommon.OutageDatabaseModel;
using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceContracts.CalculationEngine;
using Outage.Common.ServiceContracts.GDA;
using Outage.Common.ServiceContracts.PubSub;
using Outage.Common.ServiceContracts.SCADA;
using Outage.Common.ServiceProxies;
using Outage.Common.ServiceProxies.CalcualtionEngine;
using Outage.Common.ServiceProxies.PubSub;
using OutageDatabase.Repository;
using OutageManagementService.ScadaSubscriber;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using AutoResetEvent = System.Threading.AutoResetEvent;
using Timer = System.Timers.Timer;

namespace OutageManagementService.LifeCycleServices
{

    public class IsolateOutageService
    {
        private ILogger logger;

        private ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }
        private UnitOfWork dbContext;
        private OutageModel outageModel;
        public OutageMessageMapper outageMessageMapper;
        private ProxyFactory proxyFactory;


        public IsolateOutageService(OutageModel outageModel)
        {
            this.outageModel = outageModel;
            dbContext = outageModel.dbContext;
            outageMessageMapper = new OutageMessageMapper();
            proxyFactory = new ProxyFactory();
        }
        public void IsolateOutage(long outageId)
    {
        //bool success = false;
        OutageEntity outageToIsolate = dbContext.OutageRepository.Get(outageId);

        if (outageToIsolate != null)
        {
            if (outageToIsolate.OutageState == OutageState.CREATED)
            {

                Task.Run(() => StartIsolationAlgorthm(outageToIsolate))
                    .ContinueWith(task =>
                    {
                        if (task.Result)
                        {
                            dbContext.Complete();
                        }
                    }, TaskContinuationOptions.OnlyOnRanToCompletion)
                    .ContinueWith(task =>
                    {
                        try
                        {
                            outageModel.PublishOutage(Topic.ACTIVE_OUTAGE, outageMessageMapper.MapOutageEntity(outageToIsolate));
                            Logger.LogInfo($"Outage with id: 0x{outageToIsolate.OutageId:x16} is successfully published.");
                        }
                        catch (Exception e)
                        {
                                    //TODO: mozda publish neke greske??
                                    Logger.LogError("Error occured while trying to publish outage.", e);
                        }
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
            }
            else
            {
                Logger.LogWarn($"Outage with id 0x{outageId:X16} is in state {outageToIsolate.OutageState}, and thus cannot be isolated.");
            }
        }
        else
        {
            Logger.LogWarn($"Outage with id 0x{outageId:X16} is not found in database.");
            //success = false;
        }

        //return success;
    }

        public bool StartIsolationAlgorthm(OutageEntity outageToIsolate)
        {
            List<long> defaultIsolationPoints = outageToIsolate.DefaultIsolationPoints.Select(point => point.EquipmentId).ToList();

            bool isIsolated;
            bool isFirstBreakerRecloser;
            if (defaultIsolationPoints.Count > 0 && defaultIsolationPoints.Count < 3)
            {
                long headBreaker = -1;
                long recloser = -1;
                try
                {
                    isFirstBreakerRecloser = CheckIfBreakerIsRecloser(defaultIsolationPoints[0]);
                    //TODO is second recloser (future)

                }
                catch (Exception e)
                {
                    Logger.LogWarn("Exception on method CheckIfBreakerIsRecloser()", e);
                    throw e;

                }

                GetHeadBreakerAndRecloser(defaultIsolationPoints, isFirstBreakerRecloser, ref headBreaker, ref recloser);


                if (headBreaker != -1)
                {
                    ModelCode mc = outageModel.modelResourcesDesc.GetModelCodeFromId(headBreaker);
                    if (mc == ModelCode.BREAKER)
                    {
                        long headBreakerMeasurementId, recloserMeasurementId;
                        using (MeasurementMapProxy measurementMapProxy = proxyFactory.CreateProxy<MeasurementMapProxy, IMeasurementMapContract>(EndpointNames.MeasurementMapEndpoint))
                        {
                            try
                            {
                                headBreakerMeasurementId = measurementMapProxy.GetMeasurementsOfElement(headBreaker)[0];
                                if (recloser != -1)
                                {
                                    recloserMeasurementId = measurementMapProxy.GetMeasurementsOfElement(recloser)[0];
                                }
                                else
                                {
                                    recloserMeasurementId = -1;
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.LogError("Error on GetMeasurementsForElement() method.", e);
                                throw e;
                            }
                        }

                        Logger.LogInfo($"Head breaker id: 0x{headBreaker:X16}, recloser id: 0x{recloser:X16} (-1 if no recloser).");

                        //ALGORITHM
                        AutoResetEvent autoResetEvent = new AutoResetEvent(false);
                        CancelationObject cancelationObject = new CancelationObject() { CancelationSignal = false };
                        Timer timer = InitalizeAlgorthmTimer(cancelationObject, autoResetEvent);
                        ScadaNotification scadaNotification = new ScadaNotification("OutageModel_SCADA_Subscriber", new OutageIsolationAlgorithm.OutageIsolationAlgorithmParameters(headBreakerMeasurementId, recloserMeasurementId, autoResetEvent));
                        SubscriberProxy subscriberProxy = proxyFactory.CreateProxy<SubscriberProxy, ISubscriber>(scadaNotification, EndpointNames.SubscriberEndpoint);
                        subscriberProxy.Subscribe(Topic.SWITCH_STATUS);

                        long currentBreakerId = headBreaker;

                        while (!cancelationObject.CancelationSignal)
                        {
                            if (outageModel.TopologyModel.OutageTopology.ContainsKey(currentBreakerId))
                            {
                                currentBreakerId = outageModel.GetNextBreaker(currentBreakerId);
                                Logger.LogDebug($"Next breaker is 0x{currentBreakerId:X16}.");

                                if (currentBreakerId == -1 || currentBreakerId == recloser)
                                {
                                    //TODO: planned outage
                                    string message = "End of the feeder, no outage detected.";
                                    Logger.LogWarn(message);
                                    isIsolated = false;
                                    subscriberProxy.Close();
                                    outageModel.commandedElements.Clear();
                                    throw new Exception(message);
                                }
                                //TODO: SCADACommand
                                SendSCADACommand(currentBreakerId, DiscreteCommandingType.OPEN);
                                SendSCADACommand(headBreaker, DiscreteCommandingType.CLOSE);

                                timer.Start();
                                Logger.LogDebug("Timer started.");
                                autoResetEvent.WaitOne();
                                if (timer.Enabled)
                                {
                                    timer.Stop();
                                    Logger.LogDebug("Timer stoped");
                                    SendSCADACommand(currentBreakerId, DiscreteCommandingType.CLOSE);
                                }
                            }
                        }

                        long nextBreakerId = outageModel.GetNextBreaker(currentBreakerId);
                        if (currentBreakerId != 0 && currentBreakerId != recloser)
                        {
                            outageToIsolate.OptimumIsolationPoints = outageModel.GetEquipmentEntity(new List<long> { currentBreakerId, nextBreakerId });


                            if (!outageModel.TopologyModel.OutageTopology.ContainsKey(nextBreakerId))
                            {
                                string message = $"Breaker (next breaker) with id: 0x{nextBreakerId:X16} is not in topology";
                                Logger.LogError(message);
                                throw new Exception(message);
                            }

                            long outageElement = outageModel.TopologyModel.OutageTopology[nextBreakerId].FirstEnd;

                            if (!outageModel.TopologyModel.OutageTopology[currentBreakerId].SecondEnd.Contains(outageElement))
                            {
                                string message = $"Outage element with gid: 0x{outageElement:X16} is not on a second end of current breaker id";
                                Logger.LogError(message);
                                throw new Exception(message);
                            }
                            //TODO: SCADA Command
                            subscriberProxy.Close();
                            outageModel.optimumIsolationPoints.Add(currentBreakerId);
                            outageModel.optimumIsolationPoints.Add(nextBreakerId);
                            SendSCADACommand(currentBreakerId, DiscreteCommandingType.OPEN);
                            SendSCADACommand(nextBreakerId, DiscreteCommandingType.OPEN);

                            outageToIsolate.IsolatedTime = DateTime.UtcNow;
                            outageToIsolate.OutageElementGid = outageElement;
                            outageToIsolate.OutageState = OutageState.ISOLATED;

                            Logger.LogInfo($"Isolation of outage with id {outageToIsolate.OutageId}. Optimum isolation points: 0x{currentBreakerId:X16} and 0x{nextBreakerId:X16}, and outage element id is 0x{outageElement:X16}");
                            isIsolated = true;
                        }
                        else
                        {
                            string message = "End of the feeder, no outage detected.";
                            Logger.LogWarn(message);
                            isIsolated = false;
                            subscriberProxy.Close();
                            outageModel.commandedElements.Clear();
                            throw new Exception(message);
                        }

                    }
                    else
                    {
                        Logger.LogWarn($"Head breaker type is {mc}, not a {ModelCode.BREAKER}.");
                        isIsolated = false;
                    }

                }
                else
                {
                    Logger.LogWarn("Head breaker not found.");
                    isIsolated = false;
                }
            }
            else
            {
                Logger.LogWarn($"Number of defaultIsolationPoints ({defaultIsolationPoints.Count}) is out of range [1, 2].");
                isIsolated = false;
            }


            outageModel.commandedElements.Clear();
            return isIsolated;
        }
        public bool CheckIfBreakerIsRecloser(long elementId)
        {
            bool isRecloser = false;

            try
            {
                using (NetworkModelGDAProxy gda = proxyFactory.CreateProxy<NetworkModelGDAProxy, INetworkModelGDAContract>(EndpointNames.NetworkModelGDAEndpoint))
                {
                    ResourceDescription resourceDescription = gda.GetValues(elementId, new List<ModelCode>() { ModelCode.BREAKER_NORECLOSING });

                    Property property = resourceDescription.GetProperty(ModelCode.BREAKER_NORECLOSING);

                    if (property != null)
                    {
                        isRecloser = !property.AsBool();
                    }
                    else
                    {
                        throw new Exception($"Element with id 0x{elementId:X16} is not a breaker.");
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return isRecloser;
        }
        private void GetHeadBreakerAndRecloser(List<long> defaultIsolationPoints, bool isFirstBreakerRecloser, ref long headBreaker, ref long recloser)
        {
            if (defaultIsolationPoints.Count == 2)
            {
                if (isFirstBreakerRecloser)
                {
                    headBreaker = defaultIsolationPoints[1];
                    recloser = defaultIsolationPoints[0];
                }
                else
                {
                    headBreaker = defaultIsolationPoints[0];
                    recloser = defaultIsolationPoints[1];
                }
                outageModel.commandedElements.Add(headBreaker);
                outageModel.commandedElements.Add(recloser);
            }
            else
            {
                if (!isFirstBreakerRecloser)
                {
                    headBreaker = defaultIsolationPoints[0];
                    outageModel.commandedElements.Add(headBreaker);
                }
                else
                {
                    Logger.LogWarn($"Invalid state: breaker with id 0x{defaultIsolationPoints[0]:X16} is the only default isolation element, but it is also a recloser.");
                }
            }
        }
        private Timer InitalizeAlgorthmTimer(CancelationObject cancelationSignal, AutoResetEvent autoResetEvent)
        {
            Timer timer = new Timer();
            timer.Elapsed += (sender, e) => AlgorthmTimerElapsedCallback(sender, e, cancelationSignal, autoResetEvent);
            timer.Interval = 10000;
            timer.AutoReset = false;

            return timer;
        }

        private void AlgorthmTimerElapsedCallback(object sender, ElapsedEventArgs e, CancelationObject cancelationSignal, AutoResetEvent autoResetEvent)
        {
            Logger.LogDebug("Timer elapsed.");
            cancelationSignal.CancelationSignal = true;
            autoResetEvent.Set();
        }
        private void SendSCADACommand(long currentBreakerId, DiscreteCommandingType discreteCommandingType)
        {
            long measrement = -1;
            using (MeasurementMapProxy measurementMapProxy = proxyFactory.CreateProxy<MeasurementMapProxy, IMeasurementMapContract>(EndpointNames.MeasurementMapEndpoint))
            {
                List<long> measuremnts = new List<long>();
                try
                {
                    measuremnts = measurementMapProxy.GetMeasurementsOfElement(currentBreakerId);

                }
                catch (Exception e)
                {
                    //Logger.LogError("Error on GetMeasurementsForElement() method", e);
                    throw e;
                }

                if (measuremnts.Count > 0)
                {
                    measrement = measuremnts[0];
                }

            }

            if (measrement != -1)
            {
                if (discreteCommandingType == DiscreteCommandingType.OPEN && !outageModel.commandedElements.Contains(currentBreakerId))
                {
                    //TODO: add at list
                    outageModel.commandedElements.Add(currentBreakerId);
                }


                using (SCADACommandProxy scadaCommandProxy = proxyFactory.CreateProxy<SCADACommandProxy, ISCADACommand>(EndpointNames.SCADACommandService))
                {
                    try
                    {
                        bool success = scadaCommandProxy.SendDiscreteCommand(measrement, (ushort)discreteCommandingType, CommandOriginType.ISOLATING_ALGORITHM_COMMAND);
                    }
                    catch (Exception e)
                    {
                        if (discreteCommandingType == DiscreteCommandingType.OPEN && outageModel.commandedElements.Contains(currentBreakerId))
                        {
                            outageModel.commandedElements.Remove(currentBreakerId);
                        }
                        throw e;
                    }
                }
            }
        }

    }
}
