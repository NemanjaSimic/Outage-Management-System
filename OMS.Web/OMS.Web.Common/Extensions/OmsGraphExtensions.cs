namespace OMS.Web.Common.Extensions
{
    using OMS.Web.UI.Models.ViewModels;
    using System.Collections.Generic;
    using System.Linq;

    public static class OmsGraphExtensions
    {
        public const string PowerTransformerDmsTypeName = "POWERTRANSFORMER";
        public const string TransformerWindingDmsTypeName = "TRANSFORMERWINDING";

        public static OmsGraphViewModel SquashTransformerWindings(this OmsGraphViewModel graph)
        {
            IEnumerable<TransformerNodeViewModel> transformerNodes
                = graph
                .Nodes
                .Where(x => x.DMSType.Equals(PowerTransformerDmsTypeName))
                ?.Select(n => n as TransformerNodeViewModel)
                ?.ToList();

            IEnumerable<NodeViewModel> windingNodes
                = graph
                .Nodes
                .Where(x => x.DMSType.Equals(TransformerWindingDmsTypeName))
                ?.ToList();

            IEnumerable<RelationViewModel> firstWindingRelations
                = graph
                .Relations
                .Where(x => windingNodes.Any(w => w.Id.Equals(x.SourceNodeId)))
                .Where(x => transformerNodes.Any(t => t.Id.Equals(x.TargetNodeId)))
                ?.ToList();

            IEnumerable<RelationViewModel> secondWindingRelations
                = graph
                .Relations
                .Where(x => windingNodes.Any(w => w.Id.Equals(x.TargetNodeId)))
                .Where(x => transformerNodes.Any(t => t.Id.Equals(x.SourceNodeId)))
                ?.ToList();

            IEnumerable<RelationViewModel> firstWindingSourceRelations
                = graph
                .Relations
                .Where(x => firstWindingRelations.Any(f => f.SourceNodeId.Equals(x.TargetNodeId)))
                ?.Select(x =>
                    new RelationViewModel
                    {
                        SourceNodeId = x.SourceNodeId,
                        TargetNodeId = firstWindingRelations
                                        ?.First(f => f.SourceNodeId.Equals(x.TargetNodeId))
                                        ?.TargetNodeId
                    }
                )
                ?.ToList();

            IEnumerable<RelationViewModel> secondWindingTargetRelations
                = graph
                .Relations
                .Where(x => secondWindingRelations.Any(f => f.TargetNodeId.Equals(x.SourceNodeId)))
                ?.Select(x =>
                    new RelationViewModel
                    {
                        TargetNodeId = x.TargetNodeId,
                        SourceNodeId = secondWindingRelations
                                        ?.First(f => f.TargetNodeId.Equals(x.SourceNodeId))
                                        ?.SourceNodeId
                    }
                )
                ?.ToList();

            IEnumerable<RelationViewModel> windingRelationsForDelete
                = graph
                .Relations
                .Where(x => windingNodes
                            .Any(w =>
                                 x.SourceNodeId.Equals(w.Id)
                                 || x.TargetNodeId.Equals(w.Id))
                )
                .ToList();

            graph.ResolveWindings(firstWindingRelations, secondWindingRelations);
    
            graph.RemoveNodes(windingNodes);

            graph.RemoveRelations(firstWindingRelations);
            graph.RemoveRelations(secondWindingRelations);
            graph.RemoveRelations(windingRelationsForDelete);

            graph.AddRelations(firstWindingSourceRelations);
            graph.AddRelations(secondWindingTargetRelations);

            return graph;
        }

        public static OmsGraphViewModel RemoveNodes(this OmsGraphViewModel graph, IEnumerable<NodeViewModel> nodes)
        {
            foreach (var node in nodes)
                graph.Nodes.Remove(node);

            return graph;
        }

        public static OmsGraphViewModel AddRelations(this OmsGraphViewModel graph, IEnumerable<RelationViewModel> relations)
        {
            foreach (var relation in relations)
                graph.Relations.Add(relation);

            return graph;
        }

        public static OmsGraphViewModel RemoveRelations(this OmsGraphViewModel graph, IEnumerable<RelationViewModel> relations)
        {
            foreach (var relation in relations)
                graph.Relations.Remove(relation);

            return graph;
        }

        public static OmsGraphViewModel ResolveWindings(
            this OmsGraphViewModel graph,
            IEnumerable<RelationViewModel> firstWindingRelations,
            IEnumerable<RelationViewModel> secondWindingRelations)
        {
            foreach (var firstWindingRelation in firstWindingRelations)
            {
                NodeViewModel firstWinding
                    = graph
                    .Nodes
                    .First(x => x.Id.Equals(firstWindingRelation.SourceNodeId));

                TransformerNodeViewModel transformer
                    = graph
                    .Nodes
                    .First(x => x.Id.Equals(firstWindingRelation.TargetNodeId))
                    as TransformerNodeViewModel;

                transformer.AddFirstWinding(firstWinding);
            }

            foreach (var secondWindingRelation in secondWindingRelations)
            {
                NodeViewModel secondWinding
                    = graph
                    .Nodes
                    .First(x => x.Id.Equals(secondWindingRelation.TargetNodeId));

                TransformerNodeViewModel transformer
                    = graph
                    .Nodes
                    .First(x => x.Id.Equals(secondWindingRelation.SourceNodeId))
                    as TransformerNodeViewModel;

                transformer.AddSecondWinding(secondWinding);
            }

            return graph;
        }


    }
}
