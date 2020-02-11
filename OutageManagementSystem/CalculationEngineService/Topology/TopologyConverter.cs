using CECommon;
using CECommon.Interfaces;
using CECommon.Model;
using CECommon.Providers;
using NetworkModelServiceFunctions;
using Outage.Common;
using Outage.Common.OutageService.Interface;
using Outage.Common.OutageService.Model;
using Outage.Common.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TopologyBuilder;

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
            uIModel.FirstNode = topology.FirstNode;
            stack.Push(topology.FirstNode);

            while (stack.Count > 0)
            {
                if (topology.GetElementByGid(stack.Pop(), out ITopologyElement element))
                {
                    foreach (long child in element.SecondEnd)
                    {
                        long nextElement = child;
                        if (ModelCodeHelper.ExtractTypeFromGlobalId(child) == 0)
                        {
                            try
                            {
                                topology.GetElementByGid(child, out ITopologyElement fieldElement);
                                Field field = fieldElement as Field;
                                nextElement = field.Members.First();                       
                            }
                            catch (Exception)
                            {
                                logger.LogError($"[TopologyConverter] Error while getting field in Topology to UIModel convert. Element is not field or field is empty.");
                            }
                        }
                        uIModel.AddRelation(element.Id, nextElement);
                        stack.Push(nextElement);
                    }
                    List<UIMeasurement> measurements = new List<UIMeasurement>();
                    DiscreteMeasurement measurement;
                    foreach (var meausrementGid in element.Measurements)
                    {
                        if (Provider.Instance.CacheProvider.TryGetDiscreteMeasurement(meausrementGid, out measurement))
                        {
                            measurements.Add(new UIMeasurement()
                            {
                                Gid = measurement.Id,
                                Type = measurement.GetMeasurementType(),
                                Value = measurement.GetCurrentVaule()
                            });
                        }
                        else
                        {
                            logger.LogWarn($"[Topology converter] Measurement with GID {meausrementGid.ToString("X")} does not exist.");
                        }
                    }

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
                else
                {
                    logger.LogWarn($"[TopologyConverter] Error while getting topology element from topology. It does not exist in topology.");
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
            outageTopologyModel.FirstNode = topology.FirstNode;
            stack.Push(topology.FirstNode);

            while (stack.Count > 0)
            {
                if (topology.GetElementByGid(stack.Pop(), out ITopologyElement element))
                {
                    outageTopologyModel.AddElement(
                        new OutageTopologyElement(element.Id)
                        {
                            FirstEnd = element.FirstEnd,
                            DmsType = element.DmsType,
                            IsRemote = element.IsRemote,
                            SecondEnd = element.SecondEnd
                        });

                    var children = new List<long>(element.SecondEnd);

                    foreach (long child in children)
                    {
                        long nextElement = child;
                        if (ModelCodeHelper.ExtractTypeFromGlobalId(child) == 0)
                        {
                            try
                            {
                                topology.GetElementByGid(child, out ITopologyElement fieldElement);
                                Field field = fieldElement as Field;
                                nextElement = field.Members.First();
                                element.SecondEnd.Remove(child);
                                element.SecondEnd.Add(nextElement);
                            }
                            catch (Exception)
                            {
                                logger.LogError($"[TopologyConverter] Error while getting field in Topology to OMSModel convert. Element is not field or field is empty.");
                            }
                        }
                        stack.Push(nextElement);
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
