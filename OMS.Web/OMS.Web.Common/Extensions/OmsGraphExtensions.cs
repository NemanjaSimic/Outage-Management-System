namespace OMS.Web.Common.Extensions
{
    using OMS.Web.UI.Models.ViewModels;
    using System.Collections.Generic;
    using System.Linq;

    public static class OmsGraphExtensions
    {
        public const string PowerTransformerDmsTypeName = "POWERTRANSFORMER";
        public const string TransformerWindingDmsTypeName = "TRANSFORMERWINDING";

        public static OmsGraph SquashTransformerWindings(this OmsGraph graph)
        {
            // think think think!!
            // easter egg za ovaj commit: https://i.kym-cdn.com/entries/icons/original/000/030/338/New.jpg

            IEnumerable<TransformerNode> transformerNodes
                = graph
                .Nodes
                .Where(x => x.DMSType.Equals(PowerTransformerDmsTypeName))
                ?.Select(n => n as TransformerNode)
                ?.ToList();

            IEnumerable<Node> windingNodes
                = graph
                .Nodes
                .Where(x => x.DMSType.Equals(TransformerWindingDmsTypeName))
                ?.ToList();

            IEnumerable<Relation> firstWindingRelations
                = graph
                .Relations
                .Where(x => windingNodes.Any(w => w.Id.Equals(x.SourceNodeId)))
                .Where(x => transformerNodes.Any(t => t.Id.Equals(x.TargetNodeId)))
                ?.ToList();

            IEnumerable<Relation> secondWindingRelations
                = graph
                .Relations
                .Where(x => windingNodes.Any(w => w.Id.Equals(x.TargetNodeId)))
                .Where(x => transformerNodes.Any(t => t.Id.Equals(x.SourceNodeId)))
                ?.ToList();

            IEnumerable<Relation> firstWindingSourceRelations
                = graph
                .Relations
                .Where(x => firstWindingRelations.Any(f => f.SourceNodeId.Equals(x.TargetNodeId)))
                ?.Select(x =>
                    new Relation
                    {
                        SourceNodeId = x.SourceNodeId,
                        TargetNodeId = firstWindingRelations
                                        ?.First(f => f.SourceNodeId.Equals(x.TargetNodeId))
                                        ?.TargetNodeId
                    }
                )
                ?.ToList();

            IEnumerable<Relation> secondWindingTargetRelations
                = graph
                .Relations
                .Where(x => secondWindingRelations.Any(f => f.TargetNodeId.Equals(x.SourceNodeId)))
                ?.Select(x =>
                    new Relation
                    {
                        TargetNodeId = x.TargetNodeId,
                        SourceNodeId = secondWindingRelations
                                        ?.First(f => f.TargetNodeId.Equals(x.SourceNodeId))
                                        ?.SourceNodeId
                    }
                )
                ?.ToList();

            IEnumerable<Relation> windingRelationsForDelete
                = graph
                .Relations
                .Where(x => windingNodes
                            .Any(w =>
                                 x.SourceNodeId.Equals(w.Id)
                                 || x.TargetNodeId.Equals(w.Id))
                )
                .ToList();

            graph.RemoveNodes(windingNodes);

            graph.RemoveRelations(firstWindingRelations);
            graph.RemoveRelations(secondWindingRelations);
            graph.RemoveRelations(windingRelationsForDelete);

            graph.AddRelations(firstWindingSourceRelations);
            graph.AddRelations(secondWindingTargetRelations);

            return graph;
        }

        public static OmsGraph RemoveNodes(this OmsGraph graph, IEnumerable<Node> nodes)
        {
            foreach (var node in nodes)
                graph.Nodes.Remove(node);

            return graph;
        }

        public static OmsGraph AddRelations(this OmsGraph graph, IEnumerable<Relation> relations)
        {
            foreach (var relation in relations)
                graph.Relations.Add(relation);

            return graph;
        }

        public static OmsGraph RemoveRelations(this OmsGraph graph, IEnumerable<Relation> relations)
        {
            foreach (var relation in relations)
                graph.Relations.Remove(relation);

            return graph;
        }
    }
}
