using OMS.OutageSimulator.BindingModels;
using Outage.Common;
using Outage.Common.PubSub;
using Outage.Common.PubSub.SCADADataContract;
using Outage.Common.ServiceContracts.CalculationEngine;
using Outage.Common.ServiceContracts.PubSub;
using Outage.Common.ServiceContracts.SCADA;
using Outage.Common.ServiceProxies;
using Outage.Common.ServiceProxies.CalcualtionEngine;
using Outage.Common.ServiceProxies.SCADA;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OMS.OutageSimulator.ScadaSubscriber
{
    public class ScadaNotification : ISubscriberCallback
    {
        private ILogger logger;

        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private ProxyFactory proxyFactory;
        private string subscriberName;

        private long outageElementId;
        
        //KEY => MEASUREMENT ID
        private Dictionary<long, DiscreteModbusData> optimumIsolationPointsModbusData;
        //KEY => MEASUREMENT ID
        private Dictionary<long, DiscreteModbusData> defaultIsolationPointsModbusData;
        //KEY => MEASUREMENT ID
        private Dictionary<long, long> defaultToOptimumIsolationPointMap;

        public ScadaNotification(string subscriberName, ActiveOutageBindingModel outage)
        {
            this.subscriberName = subscriberName;
            this.outageElementId = outage.OutageElement.GID;
            this.proxyFactory = new ProxyFactory();

            optimumIsolationPointsModbusData = new Dictionary<long, DiscreteModbusData>(outage.OptimumIsolationPoints.Count);
            defaultIsolationPointsModbusData = new Dictionary<long, DiscreteModbusData>(outage.DefaultIsolationPoints.Count);
            defaultToOptimumIsolationPointMap = new Dictionary<long, long>(outage.DefaultToOptimumIsolationPointMap.Count);

            GetInegrityUpdateFromScada(outage);
        }

        public string GetSubscriberName()
        {
            return subscriberName;
        }

        public void Notify(IPublishableMessage message)
        {
            if(message is MultipleDiscreteValueSCADAMessage multipleDiscreteValueSCADAMessage)
            {
                //UPDATE

                
                foreach(long gid in multipleDiscreteValueSCADAMessage.Data.Keys)
                {
                    if(optimumIsolationPointsModbusData.ContainsKey(gid))
                    {
                        if (multipleDiscreteValueSCADAMessage.Data[gid].Value != optimumIsolationPointsModbusData[gid].Value)
                        {
                            optimumIsolationPointsModbusData[gid] = multipleDiscreteValueSCADAMessage.Data[gid];
                        }
                    }
                }

                foreach (long gid in multipleDiscreteValueSCADAMessage.Data.Keys)
                {
                    if (defaultIsolationPointsModbusData.ContainsKey(gid))
                    {
                        if (multipleDiscreteValueSCADAMessage.Data[gid].Value != defaultIsolationPointsModbusData[gid].Value)
                        {
                            defaultIsolationPointsModbusData[gid] = multipleDiscreteValueSCADAMessage.Data[gid];
                        }
                    }
                }

                //COMMAND OPEN IF TRUE
                Dictionary<long, DiscreteModbusData> defaultIsolationPointsToBeOpened = new Dictionary<long, DiscreteModbusData>();

                foreach (long gid in defaultIsolationPointsModbusData.Keys)
                {
                    if (defaultToOptimumIsolationPointMap.ContainsKey(gid))
                    {
                        long optimumPointGid = defaultToOptimumIsolationPointMap[gid];

                        if (optimumIsolationPointsModbusData.ContainsKey(optimumPointGid))
                        {
                            ushort optimumIsolationPointValue = optimumIsolationPointsModbusData[optimumPointGid].Value;

                            if (optimumIsolationPointValue == (ushort)DiscreteCommandingType.CLOSE)
                            {
                                defaultIsolationPointsToBeOpened.Add(gid, defaultIsolationPointsModbusData[gid]);
                            }
                        }
                    }
                }

                OpenIsolationPoints(defaultIsolationPointsToBeOpened);
            }
            else
            {
                string errorMessage = "SCADA returned wrong value for in SCADAPublication. MultipleDiscreteValueSCADAMessage excepted.";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }
        }

        private void GetInegrityUpdateFromScada(ActiveOutageBindingModel outage)
        {
            using (SCADAIntegrityUpdateProxy proxy = proxyFactory.CreateProxy<SCADAIntegrityUpdateProxy, ISCADAIntegrityUpdateContract>(EndpointNames.SCADAIntegrityUpdateEndpoint))
            {
                if(proxy == null)
                {
                    string message = "GetInegrityUpdateFromScada => SCADAIntegrityUpdateProxy is null";
                    Logger.LogError(message);
                    throw new NullReferenceException(message);
                }

                SCADAPublication data = proxy.GetIntegrityUpdateForSpecificTopic(Topic.SWITCH_STATUS);
                
                if(data.Message is MultipleDiscreteValueSCADAMessage multipleDiscreteValueSCADAMessage)
                {
                    using (MeasurementMapProxy measurementProxy = proxyFactory.CreateProxy<MeasurementMapProxy, IMeasurementMapContract>(EndpointNames.MeasurementMapEndpoint))
                    {
                        if (measurementProxy == null)
                        {
                            string errorMessage = "Notify => MeasurementMapProxy is null";
                            Logger.LogError(errorMessage);
                            throw new NullReferenceException(errorMessage);
                        }

                        Dictionary<long, List<long>> elemetnToMeasurementMap = measurementProxy.GetElementToMeasurementMap();

                        //UPDATE

                        foreach(long optimumIsolationPointElementId in outage.OptimumIsolationPoints.Select(p => p.GID))
                        {
                            if(!elemetnToMeasurementMap.ContainsKey(optimumIsolationPointElementId))
                            {
                                continue;
                            }

                            List<long> optimumIsolationPointMeasurementIds = elemetnToMeasurementMap[optimumIsolationPointElementId];

                            foreach(long optimumIsolationPointMeasurementId in optimumIsolationPointMeasurementIds)
                            {
                                if (multipleDiscreteValueSCADAMessage.Data.ContainsKey(optimumIsolationPointMeasurementId))
                                {
                                    if (!optimumIsolationPointsModbusData.ContainsKey(optimumIsolationPointMeasurementId))
                                    {
                                        optimumIsolationPointsModbusData.Add(optimumIsolationPointMeasurementId, multipleDiscreteValueSCADAMessage.Data[optimumIsolationPointMeasurementId]);
                                    }
                                }
                            }
                        }

                        foreach (long defaultIsolationPointElementId in outage.DefaultIsolationPoints.Select(p => p.GID))
                        {
                            if (!elemetnToMeasurementMap.ContainsKey(defaultIsolationPointElementId))
                            {
                                continue;
                            }

                            List<long> defaultIsolationPointMeasurementIds = elemetnToMeasurementMap[defaultIsolationPointElementId];

                            foreach (long defaultIsolationPointMeasuremetId in defaultIsolationPointMeasurementIds)
                            {
                                if (multipleDiscreteValueSCADAMessage.Data.ContainsKey(defaultIsolationPointMeasuremetId))
                                {
                                    if (!defaultIsolationPointsModbusData.ContainsKey(defaultIsolationPointMeasuremetId))
                                    {
                                        defaultIsolationPointsModbusData.Add(defaultIsolationPointMeasuremetId, multipleDiscreteValueSCADAMessage.Data[defaultIsolationPointMeasuremetId]);
                                    }
                                }
                            }

                            if(outage.DefaultToOptimumIsolationPointMap.ContainsKey(defaultIsolationPointElementId))
                            {
                                long optimumIsolationPointElementId = outage.DefaultToOptimumIsolationPointMap[defaultIsolationPointElementId];

                                if (elemetnToMeasurementMap.ContainsKey(optimumIsolationPointElementId))
                                {
                                    long optimumIsolationPointMeasurementGid = elemetnToMeasurementMap[outage.DefaultToOptimumIsolationPointMap[defaultIsolationPointElementId]].First(); //TODO: ko voli nek izvoli
                                    long defaultIsolationPointMeasurementGid = defaultIsolationPointMeasurementIds.First(); //TODO: ko voli nek izvoli

                                    if(!defaultToOptimumIsolationPointMap.ContainsKey(defaultIsolationPointMeasurementGid))
                                    {
                                        defaultToOptimumIsolationPointMap.Add(defaultIsolationPointMeasurementGid, optimumIsolationPointMeasurementGid);
                                    }
                                }
                            }
                        }

                        //COMMAND OPEN IF TRUE
                        Dictionary<long, DiscreteModbusData> defaultIsolationPointsToBeOpened = new Dictionary<long, DiscreteModbusData>();

                        foreach (long gid in defaultIsolationPointsModbusData.Keys)
                        { 
                            if (defaultToOptimumIsolationPointMap.ContainsKey(gid))
                            {
                                long optimumPointGid = defaultToOptimumIsolationPointMap[gid];

                                if (optimumIsolationPointsModbusData.ContainsKey(optimumPointGid))
                                {
                                    ushort optimumIsolationPointValue = optimumIsolationPointsModbusData[optimumPointGid].Value;
                                    
                                    if(optimumIsolationPointValue == (ushort)DiscreteCommandingType.CLOSE)
                                    {
                                        defaultIsolationPointsToBeOpened.Add(gid, defaultIsolationPointsModbusData[gid]);
                                    }
                                }
                            }
                        }

                        OpenIsolationPoints(defaultIsolationPointsToBeOpened);
                    }
                }
                else
                {
                    string message = "SCADA returned wrong value for in SCADAPublication. MultipleDiscreteValueSCADAMessage excepted.";
                    throw new Exception(message);
                }
            }
        }

        private void OpenIsolationPoints(Dictionary<long, DiscreteModbusData> isolationPoints)
        {
            using(SCADACommandProxy proxy = proxyFactory.CreateProxy<SCADACommandProxy, ISCADACommand>(EndpointNames.SCADACommandService))
            {
                if (proxy == null)
                {
                    string message = "OpenDefaultIsolationPoints => SCADACommandProxy is null";
                    Logger.LogError(message);
                    throw new NullReferenceException(message);
                }

                foreach(long gid in isolationPoints.Keys)
                {
                    if(isolationPoints[gid].Value != (ushort)DiscreteCommandingType.OPEN)
                    {
                        //TODO: COMMANDING ENUM u COMMON
                        proxy.SendDiscreteCommand(gid, (ushort)DiscreteCommandingType.OPEN, CommandOriginType.OUTAGE_SIMULATOR);
                    }
                }
            }
        }
    }
}
