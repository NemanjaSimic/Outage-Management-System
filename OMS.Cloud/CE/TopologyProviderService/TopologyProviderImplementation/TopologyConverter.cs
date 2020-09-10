using Common.CeContracts;
using Common.CeContracts.ModelProvider;
using Common.PubSubContracts.DataContracts.CE;
using Common.PubSubContracts.DataContracts.CE.UIModels;
using OMS.Common.Cloud.Logger;
using OMS.Common.NmsContracts;
using OMS.Common.WcfClient.CE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CE.TopologyProviderImplementation
{
    public class TopologyConverter : ITopologyConverterContract
    {
        private readonly string baseLogString;

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public TopologyConverter()
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            string verboseMessage = $"{baseLogString} entering Ctor.";
            Logger.LogVerbose(verboseMessage);

            string debugMessage = $"{baseLogString} Ctor => Clients initialized.";
            Logger.LogDebug(debugMessage);
        }

        public async Task<UIModel> ConvertTopologyToUIModel(TopologyModel topology)
        {
            string verboseMessage = $"{baseLogString} ConvertTopologyToUIModel method called.";
            Logger.LogVerbose(verboseMessage);

            if (topology == null)
            {
                string message = $"{baseLogString} ConvertTopologyToUIModel => Provider topology is null.";
                Logger.LogError(message);
                throw new Exception(message);
            }

            UIModel uIModel = new UIModel();
            Stack<long> stack = new Stack<long>();

            Logger.LogDebug($"{baseLogString} ConvertTopologyToUIModel => Calling GetReclosers method from model provider client.");
            var modelProviderClient = ModelProviderClient.CreateClient();
            var reclosers = await modelProviderClient.GetReclosers();
            Logger.LogDebug($"{baseLogString} ConvertTopologyToUIModel => GetReclosers method from model provider client has been called successfully.");

            uIModel.FirstNode = topology.FirstNode;
            stack.Push(topology.FirstNode);
            long nextElementGid;
            
            while (stack.Count > 0)
            {
                nextElementGid = stack.Pop();
                if (topology.GetElementByGid(nextElementGid, out ITopologyElement element))
                {
                    if (!reclosers.Contains(nextElementGid))
                    {
                        foreach (var child in element.SecondEnd)
                        {
                            long nextElement = child.Id;
                            if (ModelCodeHelper.ExtractTypeFromGlobalId(child.Id) == 0)
                            {
                                if (child is Field field && field.Members.Count > 0)
                                {
                                    nextElement = field.Members.First().Id;
                                }
                                else
                                {
                                    string message = $"{baseLogString} ConvertTopologyToUIModel => Error while getting field in Topology to UIModel convert. Element is not field or field is empty.";
                                    Logger.LogError(message);
                                    throw new Exception(message);
                                }
                            }

                            uIModel.AddRelation(element.Id, nextElement);
                            stack.Push(nextElement);
                        }
                    }

                    List<UIMeasurement> measurements = new List<UIMeasurement>();
                    foreach (var measurementGid in element.Measurements.Keys)
                    {
                        Logger.LogDebug($"{baseLogString} ConvertTopologyToUIModel => Calling GetDiscreteMeasurement method from measurement provider client for measurement GID {measurementGid:X16}.");
                        var measurementProviderClient = MeasurementProviderClient.CreateClient();
                        DiscreteMeasurement discreteMeasurement = await measurementProviderClient.GetDiscreteMeasurement(measurementGid);
                        Logger.LogDebug($"{baseLogString} ConvertTopologyToUIModel => GetDiscreteMeasurement method from measurement provider client has been called successfully.");

                        Logger.LogDebug($"{baseLogString} ConvertTopologyToUIModel => Calling GetAnalogMeasurement method from measurement provider client for measurement GID {measurementGid:X16}.");
                        measurementProviderClient = MeasurementProviderClient.CreateClient();
                        AnalogMeasurement analogMeasurement = await measurementProviderClient.GetAnalogMeasurement(measurementGid);
                        Logger.LogDebug($"{baseLogString} ConvertTopologyToUIModel => GetAnalogMeasurement method from measurement provider client has been called successfully.");

                        if (discreteMeasurement != null)
                        {
                            measurements.Add(new UIMeasurement()
                            {
                                Gid = discreteMeasurement.Id,
                                Type = discreteMeasurement.GetMeasurementType(),
                                Value = discreteMeasurement.GetCurrentValue()
                            });
                        }
                        else if (analogMeasurement != null)
                        {
                            measurements.Add(new UIMeasurement()
                            {
                                Gid = analogMeasurement.Id,
                                Type = analogMeasurement.GetMeasurementType(),
                                Value = analogMeasurement.GetCurrentValue()
                            });
                        }
                        else
                        {
                            Logger.LogWarning($"{baseLogString} ConvertTopologyToUIModel => Measurement with GID {measurementGid:X16} does not exist.");
                        }
                    }


                    if (!uIModel.Nodes.ContainsKey(element.Id))
                    {
                        UINode newUINode = new UINode()
                        {
                            Id = element.Id,
                            Name = element.Name,
                            Mrid = element.Mrid,
                            Description = element.Description,
                            DMSType = element.DmsType,
                            NominalVoltage = element.NominalVoltage,
                            Measurements = measurements,
                            IsActive = element.IsActive,
                            IsRemote = element.IsRemote,
                            NoReclosing = element.NoReclosing
                        };
                        uIModel.AddNode(newUINode);
                    }
                }
                else
                {
                    Logger.LogError($"{baseLogString} ConvertTopologyToUIModel => Error while getting topology element with GID {nextElementGid:X16} from topology. It does not exist in topology.");
                }
            }

            Logger.LogDebug($"{baseLogString} ConvertTopologyToUIModel => Topology to UIModel convert finished successfully.");
            return uIModel;
        }

        public async Task<OutageTopologyModel> ConvertTopologyToOMSModel(TopologyModel topology)
        {
            string verboseMessage = $"{baseLogString} ConvertTopologyToOMSModel method called.";
            Logger.LogVerbose(verboseMessage);

            if (topology == null)
            {
                string message = $"{baseLogString} ConvertTopologyToOMSModel => Provider topology is null.";
                Logger.LogError(message);
                throw new Exception(message);
            }

            Logger.LogDebug($"{baseLogString} ConvertTopologyToOMSModel => Calling GetReclosers method from model provider client.");
            var modelProviderClient = ModelProviderClient.CreateClient();
            var reclosers = await modelProviderClient.GetReclosers();
            Logger.LogDebug($"{baseLogString} ConvertTopologyToOMSModel => GetReclosers method from model provider client has been called successfully.");

            OutageTopologyModel outageTopologyModel = new OutageTopologyModel();
            Stack<long> stack = new Stack<long>();
            outageTopologyModel.FirstNode = topology.FirstNode;
            stack.Push(topology.FirstNode);

            List<long> secondEnd = new List<long>();
            long nextElement = 0;
            long nextElementGid = 0;

            ITopologyElement element;
            bool isOpen;

            while (stack.Count > 0)
            {
                nextElementGid = stack.Pop();
                if (topology.GetElementByGid(nextElementGid, out element))
                {

                    secondEnd.Clear();
                    if (!reclosers.Contains(nextElementGid))
                    {
                        foreach (var child in element.SecondEnd)
                        {
                            nextElement = child.Id;
                            if (ModelCodeHelper.ExtractTypeFromGlobalId(nextElement) == 0)
                            {
                                if (child is Field field && field.Members.Count > 0)
                                {
                                    nextElement = field.Members.First().Id;
                                }
                                else
                                {
                                    string message = $"{baseLogString} ConvertTopologyToOMSModel => Error while getting field in Topology to UIModel convert. Element is not field or field is empty.";
                                    Logger.LogError(message);
                                    throw new Exception(message);
                                }
                            }
                            secondEnd.Add(nextElement);
                            stack.Push(nextElement);
                        }
                    }

                    isOpen = false;
                    DiscreteMeasurement discreteMeasurement;
                    foreach (var measurementGid in element.Measurements.Keys)
                    {
                        Logger.LogDebug($"{baseLogString} ConvertTopologyToOMSModel => Calling GetDiscreteMeasurement method from measurement provider client for measurement GID {measurementGid:X16}.");
                        var measurementProviderClient = MeasurementProviderClient.CreateClient();
                        discreteMeasurement = await measurementProviderClient.GetDiscreteMeasurement(measurementGid);
                        Logger.LogDebug($"{baseLogString} ConvertTopologyToOMSModel => GetDiscreteMeasurement method from measurement provider client has been called successfully.");

                        if (discreteMeasurement != null)
                        {
                            isOpen = discreteMeasurement.CurrentOpen;
                        }
                        else
                        {
                            Logger.LogWarning($"{baseLogString} ConvertTopologyToOMSModel => Measurement provider client returned null for measurement GID {measurementGid:X16}.");
                        }
                    }

                    if (!outageTopologyModel.GetElementByGid(element.Id, out var _))
                    {
                        outageTopologyModel.AddElement(
                            new OutageTopologyElement(element.Id)
                            {
                                FirstEnd = (element.FirstEnd != null) ? element.FirstEnd.Id : 0,
                                DmsType = element.DmsType,
                                IsRemote = element.IsRemote,
                                IsActive = element.IsActive,
                                SecondEnd = new List<long>(secondEnd),
                                NoReclosing = element.NoReclosing,
                                IsOpen = isOpen
                            });
                    }
                }
                else
                {
                    Logger.LogError($"{baseLogString} ConvertTopologyToOMSModel => Error while getting topology element from topology. It does not exist in topology.");
                }
            }

            Logger.LogDebug($"{baseLogString} ConvertTopologyToOMSModel => Topology to OMSModel convert finished successfully.");
            return outageTopologyModel;
        }
        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
    }
}
