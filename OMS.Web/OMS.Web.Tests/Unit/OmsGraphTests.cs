namespace OMS.Web.Tests.Unit
{
    using Xunit;
    using OMS.Web.UI.Models.ViewModels;
    using OMS.Web.Common.Extensions;
    using System.Collections.Generic;

    public class OmsGraphTests
    {
        #region Node Test Data
        private readonly Node firstWinding = new Node
        {
            Id = "2",
            DMSType = "TRANSFORMERWINDING"
        };

        private readonly Node secondWinding = new Node
        {
            Id = "4",
            DMSType = "TRANSFORMERWINDING"
        };

        private readonly List<Node> initialNodes = new List<Node>
        {
            new Node { Id = "1", DMSType="ENERGYSOURCE" },
            new TransformerNode { Id = "3", DMSType="POWERTRANSFORMER" },
            new Node { Id = "5", DMSType="LOADBREAKSWITCH" }
        };

        private readonly List<Node> resultNodes = new List<Node>
        {
            new Node { Id = "1", DMSType="ENERGYSOURCE" },
            new TransformerNode { Id = "3", DMSType = "POWERTRANSFORMER" },
            new Node { Id = "5", DMSType="LOADBREAKSWITCH" }
        };
        #endregion

        #region Relations Test Data
        private readonly List<Relation> initialRelations = new List<Relation>
        {
            new Relation { SourceNodeId = "1", TargetNodeId = "2" },
            new Relation { SourceNodeId = "2", TargetNodeId = "3" },
            new Relation { SourceNodeId = "3", TargetNodeId = "4" },
            new Relation { SourceNodeId = "4", TargetNodeId = "5" }
        };

        private readonly List<Relation> resultRelations = new List<Relation>
        {
            new Relation { SourceNodeId = "1", TargetNodeId = "3" },
            new Relation { SourceNodeId = "3", TargetNodeId = "5" }
        };
        #endregion

        public OmsGraphTests()
        {
            initialNodes.Add(firstWinding);
            initialNodes.Add(secondWinding);

            ((TransformerNode)resultNodes[1]).FirstWinding = firstWinding;
            ((TransformerNode)resultNodes[1]).SecondWinding = secondWinding;
        }

        [Fact]
        public void Test_Xunit()
        {
            // Ako vam ovo ne prolazi, ne radi vam lepo Xunit
            Assert.Equal(1, 1);
        }

        [Fact]
        public void GivenTopology_WhenSquashWindings_ShouldSquash()
        {
            OmsGraph graph = new OmsGraph();
            graph.Nodes = initialNodes;
            graph.Relations = initialRelations;

            OmsGraph resultGraph = graph.SquashTransformerWindings();

            Assert.NotNull(graph);
            Assert.NotNull(resultGraph);
        }


    }
}
