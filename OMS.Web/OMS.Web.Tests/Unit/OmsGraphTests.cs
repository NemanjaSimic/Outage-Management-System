namespace OMS.Web.Tests.Unit
{
    using OMS.Web.Common.Extensions;
    using OMS.Web.UI.Models.ViewModels;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class OmsGraphTests
    {
        #region Node Test Data
        private readonly List<Node> initialNodes = new List<Node>
        {
            new Node { Id = "1", DMSType="ENERGYSOURCE" },
            new Node { Id = "2", DMSType="LOADBREAKSWITCH" },
            new Node { Id = "3", DMSType="TRANSFORMERWINDING" },
            new TransformerNode { Id = "4", DMSType="POWERTRANSFORMER" },
            new Node { Id = "5", DMSType="TRANSFORMERWINDING" },
            new Node { Id = "6", DMSType="LOADBREAKSWITCH" },
            new Node { Id = "7", DMSType="LOADBREAKSWITCH" },
            new Node { Id = "8", DMSType="TRANSFORMERWINDING" },
            new TransformerNode { Id = "9", DMSType="POWERTRANSFORMER" },
            new Node { Id = "10", DMSType="TRANSFORMERWINDING" },
            new Node { Id = "11", DMSType="LOADBREAKSWITCH" }
        };

        private readonly List<Node> resultNodes = new List<Node>
        {
            new Node { Id = "1", DMSType="ENERGYSOURCE" },
            new Node { Id = "2", DMSType="LOADBREAKSWITCH" },
            new TransformerNode { Id = "4", DMSType="POWERTRANSFORMER" },
            new Node { Id = "6", DMSType="LOADBREAKSWITCH" },
            new Node { Id = "7", DMSType="LOADBREAKSWITCH" },
            new TransformerNode { Id = "9", DMSType="POWERTRANSFORMER" },
            new Node { Id = "11", DMSType="LOADBREAKSWITCH" }
        };
        #endregion

        #region Relations Test Data
        private readonly List<Relation> initialRelations = new List<Relation>
        {
            new Relation { SourceNodeId = "1", TargetNodeId = "2" },
            new Relation { SourceNodeId = "2", TargetNodeId = "3" },
            new Relation { SourceNodeId = "3", TargetNodeId = "4" },
            new Relation { SourceNodeId = "4", TargetNodeId = "5" },
            new Relation { SourceNodeId = "5", TargetNodeId = "6" },
            new Relation { SourceNodeId = "1", TargetNodeId = "7" },
            new Relation { SourceNodeId = "7", TargetNodeId = "8" },
            new Relation { SourceNodeId = "8", TargetNodeId = "9" },
            new Relation { SourceNodeId = "9", TargetNodeId = "10" },
            new Relation { SourceNodeId = "10", TargetNodeId = "11" }
        };

        private readonly List<Relation> resultRelations = new List<Relation>
        {
            new Relation { SourceNodeId = "1", TargetNodeId = "2" },
            new Relation { SourceNodeId = "2", TargetNodeId = "4" },
            new Relation { SourceNodeId = "4", TargetNodeId = "6" },
            new Relation { SourceNodeId = "1", TargetNodeId = "7" },
            new Relation { SourceNodeId = "7", TargetNodeId = "9" },
            new Relation { SourceNodeId = "9", TargetNodeId = "11" }
        };
        #endregion

        public OmsGraphTests()
        {
            ((TransformerNode)resultNodes[2]).FirstWinding = initialNodes[2];
            ((TransformerNode)resultNodes[2]).SecondWinding = initialNodes[4];

            ((TransformerNode)resultNodes[5]).FirstWinding = initialNodes[7];
            ((TransformerNode)resultNodes[5]).SecondWinding = initialNodes[9];
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

            Assert.NotNull(resultGraph);
            Assert.Equal(resultNodes.Count, resultGraph.Nodes.Count);
            Assert.Equal(resultRelations.Count, resultGraph.Relations.Count);
            
            Assert.Equal(
                ((TransformerNode)resultNodes[2]).FirstWinding,
                ((TransformerNode)resultGraph.Nodes[2]).FirstWinding
            );

            Assert.Equal(
                ((TransformerNode)resultNodes[5]).FirstWinding,
                ((TransformerNode)resultGraph.Nodes[5]).FirstWinding
            );
        }


    }
}
