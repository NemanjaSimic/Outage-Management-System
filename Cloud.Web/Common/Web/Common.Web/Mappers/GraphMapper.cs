﻿using Common.PubSubContracts.DataContracts.CE.UIModels;
using Common.Web.Extensions;
using Common.Web.Models.ViewModels;
using OMS.Common.Cloud;
using System.Collections.Generic;

namespace Common.Web.Mappers
{
    public class GraphMapper : IGraphMapper
    {
        private const string PowerTransformerDmsTypeName = "POWERTRANSFORMER";

        public OmsGraphViewModel Map(UIModel topologyModel)
        {
            OmsGraphViewModel graph = new OmsGraphViewModel();
            
            // map nodes
            foreach (KeyValuePair<long, UINode> keyValue in topologyModel.Nodes)
            {
                NodeViewModel graphNode = new NodeViewModel
                {
                    Id = keyValue.Value.Id.ToString(),
                    Name = keyValue.Value.Name,
                    Description = keyValue.Value.Description,
                    Mrid = keyValue.Value.Mrid,
                    IsActive = keyValue.Value.IsActive,
                    DMSType = keyValue.Value.DMSType,
                    IsRemote = keyValue.Value.IsRemote,
                    NoReclosing = keyValue.Value.NoReclosing,
                    NominalVoltage = keyValue.Value.NominalVoltage.ToString(),
                    Measurements = new List<MeasurementViewModel>()
                };

                foreach (var measurement in keyValue.Value.Measurements)
                {
                    graphNode.Measurements.Add(new MeasurementViewModel()
                    {
                        Id = measurement.Gid.ToString(),
                        Type = measurement.Type,
                        Value = measurement.Value,
                        AlarmType = measurement.AlarmType
                    });
                }

                graph.Nodes.Add(
                    graphNode.DMSType == PowerTransformerDmsTypeName 
                    ? graphNode.ToTransformerNode()
                    : graphNode
                );
            }

            // map relations
            foreach (KeyValuePair<long, HashSet<long>> keyValue in topologyModel.Relations) 
            {
                foreach(long targetNodeId in keyValue.Value)
                {
                    RelationViewModel graphRelation = new RelationViewModel
                    {
                        SourceNodeId = keyValue.Key.ToString(),
                        TargetNodeId = targetNodeId.ToString(),
                        IsActive = topologyModel.Nodes[keyValue.Key].IsActive || topologyModel.Nodes[targetNodeId].IsActive
                    };

                    graph.Relations.Add(graphRelation);
                }
            }

            return graph.SquashTransformerWindings();
        }
    }
}
