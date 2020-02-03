namespace OMS.Web.Common.Mappers
{
    using OMS.Web.UI.Models.ViewModels;
    using OMS.Web.Common.Extensions;
    using Outage.Common.UI;
    using System.Collections.Generic;
    
    public class GraphMapper : IGraphMapper
    {
        private const string PowerTransformerDmsTypeName = "POWERTRANSFORMER";

        public OmsGraph MapTopology(UIModel topologyModel)
        {
            OmsGraph graph = new OmsGraph();
            
            // map nodes
            foreach (KeyValuePair<long, UINode> keyValue in topologyModel.Nodes)
            {
                Node graphNode = new Node
                {
                    Id = keyValue.Value.Id.ToString(),
                    Name = keyValue.Value.Name,
                    Description = keyValue.Value.Description,
                    Mrid = keyValue.Value.Mrid,
                    IsActive = keyValue.Value.IsActive,
                    DMSType = keyValue.Value.DMSType,
                    IsRemote = keyValue.Value.IsRemote,
                    NominalVoltage = keyValue.Value.NominalVoltage.ToString(),
                    Measurements = new List<Measurement>()
                };

                foreach (var measurement in keyValue.Value.Measurements)
                {
                    graphNode.Measurements.Add(new Measurement()
                    {
                        Id = measurement.Gid.ToString(),
                        Type = measurement.Type,
                        Value = measurement.Value
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
                    Relation graphRelation = new Relation
                    {
                        SourceNodeId = keyValue.Key.ToString(),
                        TargetNodeId = targetNodeId.ToString(),
                        IsActive = topologyModel.Nodes[keyValue.Key].IsActive
                    };

                    graph.Relations.Add(graphRelation);
                }
            }

            return graph.SquashTransformerWindings();
        }
    }
}
