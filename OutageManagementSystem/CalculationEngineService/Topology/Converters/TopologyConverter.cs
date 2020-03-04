using CECommon;
using CECommon.Interfaces;
using CECommon.Model;
using CECommon.Providers;
using Outage.Common;
using Outage.Common.OutageService.Interface;
using Outage.Common.OutageService.Model;
using Outage.Common.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Topology
{
    public class TopologyConverter : ITopologyConverter
    {
        ILogger logger = LoggerWrapper.Instance;
        public UIModel ConvertTopologyToUIModel(ITopology topology)
        {
            logger.LogDebug("Topology to UIModel convert started.");
            UIModel uIModel = new UIModel();
            Stack<long> stack = new Stack<long>();
            var reclosers = Provider.Instance.ModelProvider.GetReclosers();
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
                                try
                                {
                                    Field field = child as Field;
                                    nextElement = field.Members.First().Id;
                                }
                                catch (Exception)
                                {
                                    logger.LogError($"[TopologyConverter] Error while getting field in Topology to UIModel convert. Element is not field or field is empty.");
                                }
                            }
                            uIModel.AddRelation(element.Id, nextElement);
                            stack.Push(nextElement);
                        }
                    }
                    List<UIMeasurement> measurements = new List<UIMeasurement>();
                    foreach (var meausrementGid in element.Measurements)
                    {
                        if (Provider.Instance.MeasurementProvider.TryGetDiscreteMeasurement(meausrementGid, out DiscreteMeasurement discreteMeasurement))
                        {
                            measurements.Add(new UIMeasurement()
                            {
                                Gid = discreteMeasurement.Id,
                                Type = discreteMeasurement.GetMeasurementType(),
                                Value = discreteMeasurement.GetCurrentVaule()
                            });
                        }
                        else if (Provider.Instance.MeasurementProvider.TryGetAnalogMeasurement(meausrementGid, out AnalogMeasurement analogMeasurement))
                        {
                            measurements.Add(new UIMeasurement()
                            {
                                Gid = analogMeasurement.Id,
                                Type = analogMeasurement.GetMeasurementType(),
                                Value = analogMeasurement.GetCurrentVaule()
                            });
                        }
                        else
                        {
                            logger.LogWarn($"[Topology converter] Measurement with GID 0x{meausrementGid.ToString("X16")} does not exist.");
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
                            IsRemote = element.IsRemote
                        };
                        uIModel.AddNode(newUINode);
                    }
                }
                else
                {
                    logger.LogWarn($"[TopologyConverter - UIModel] Error while getting topology element with GID 0x{nextElementGid.ToString("X16")} from topology. It does not exist in topology.");
                }
            }
            logger.LogDebug("Topology to UIModel convert finished successfully.");
            return uIModel;
        }
        public IOutageTopologyModel ConvertTopologyToOMSModel(ITopology topology)
        {
            logger.LogDebug("Topology to OMS model convert started.");
            IOutageTopologyModel outageTopologyModel = new OutageTopologyModel();
            Stack<long> stack = new Stack<long>();

            var reclosers = Provider.Instance.ModelProvider.GetReclosers();
            outageTopologyModel.FirstNode = topology.FirstNode;
            stack.Push(topology.FirstNode);

            List<long> secondEnd = new List<long>();
            long nextElement = 0;
            long nextElementGid = 0;
            ITopologyElement element;

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
                                try
                                {
                                    Field field = child as Field;
                                    nextElement = field.Members.First().Id;
                                }
                                catch (Exception)
                                {
                                    logger.LogError($"[TopologyConverter] Error while getting field in Topology to OMSModel convert. Element is not field or field is empty.");
                                }
                            }
                            secondEnd.Add(nextElement);
                            stack.Push(nextElement);
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
                            });
                    }
                }
                else
                {
                    logger.LogWarn($"[TopologyConverter] Error while getting topology element from topology. It does not exist in topology.");
                }
            }
            logger.LogDebug("Topology to OMSModel convert finished successfully.");
            return outageTopologyModel;
        }
    }
}
