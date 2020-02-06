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
        private readonly List<NodeViewModel> initialNodes = new List<NodeViewModel>
        {
            new NodeViewModel { Id = "1", DMSType="ENERGYSOURCE" },
            new NodeViewModel { Id = "2", DMSType="LOADBREAKSWITCH" },
            new NodeViewModel { Id = "3", DMSType="TRANSFORMERWINDING" },
            new NodeViewModel { Id = "4", DMSType="POWERTRANSFORMER" },
            new NodeViewModel { Id = "5", DMSType="TRANSFORMERWINDING" },
            new NodeViewModel { Id = "6", DMSType="LOADBREAKSWITCH" },
            new NodeViewModel { Id = "7", DMSType="LOADBREAKSWITCH" },
            new NodeViewModel { Id = "8", DMSType="TRANSFORMERWINDING" },
            new NodeViewModel { Id = "9", DMSType="POWERTRANSFORMER" },
            new NodeViewModel { Id = "10", DMSType="TRANSFORMERWINDING" },
            new NodeViewModel { Id = "11", DMSType="LOADBREAKSWITCH" }
        };

        private readonly List<NodeViewModel> resultNodes = new List<NodeViewModel>
        {
            new NodeViewModel { Id = "1", DMSType="ENERGYSOURCE" },
            new NodeViewModel { Id = "2", DMSType="LOADBREAKSWITCH" },
            new TransformerNodeViewModel { Id = "4", DMSType="POWERTRANSFORMER" },
            new NodeViewModel { Id = "6", DMSType="LOADBREAKSWITCH" },
            new NodeViewModel { Id = "7", DMSType="LOADBREAKSWITCH" },
            new TransformerNodeViewModel { Id = "9", DMSType="POWERTRANSFORMER" },
            new NodeViewModel { Id = "11", DMSType="LOADBREAKSWITCH" }
        };
        #endregion

        #region Relations Test Data
        private readonly List<RelationViewModel> initialRelations = new List<RelationViewModel>
        {
            new RelationViewModel { SourceNodeId = "1", TargetNodeId = "2" },
            new RelationViewModel { SourceNodeId = "2", TargetNodeId = "3" },
            new RelationViewModel { SourceNodeId = "3", TargetNodeId = "4" },
            new RelationViewModel { SourceNodeId = "4", TargetNodeId = "5" },
            new RelationViewModel { SourceNodeId = "5", TargetNodeId = "6" },
            new RelationViewModel { SourceNodeId = "1", TargetNodeId = "7" },
            new RelationViewModel { SourceNodeId = "7", TargetNodeId = "8" },
            new RelationViewModel { SourceNodeId = "8", TargetNodeId = "9" },
            new RelationViewModel { SourceNodeId = "9", TargetNodeId = "10" },
            new RelationViewModel { SourceNodeId = "10", TargetNodeId = "11" }
        };

        private readonly List<RelationViewModel> resultRelations = new List<RelationViewModel>
        {
            new RelationViewModel { SourceNodeId = "1", TargetNodeId = "2" },
            new RelationViewModel { SourceNodeId = "2", TargetNodeId = "4" },
            new RelationViewModel { SourceNodeId = "4", TargetNodeId = "6" },
            new RelationViewModel { SourceNodeId = "1", TargetNodeId = "7" },
            new RelationViewModel { SourceNodeId = "7", TargetNodeId = "9" },
            new RelationViewModel { SourceNodeId = "9", TargetNodeId = "11" }
        };
        #endregion

        public OmsGraphTests()
        {
            initialNodes[3] = initialNodes[3].ToTransformerNode();
            initialNodes[8] = initialNodes[8].ToTransformerNode();

            ((TransformerNodeViewModel)resultNodes[2]).FirstWinding = initialNodes[2];
            ((TransformerNodeViewModel)resultNodes[2]).SecondWinding = initialNodes[4];

            ((TransformerNodeViewModel)resultNodes[5]).FirstWinding = initialNodes[7];
            ((TransformerNodeViewModel)resultNodes[5]).SecondWinding = initialNodes[9];
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
            OmsGraphViewModel graph = new OmsGraphViewModel();
            graph.Nodes = initialNodes;
            graph.Relations = initialRelations;

            OmsGraphViewModel resultGraph = graph.SquashTransformerWindings();

            Assert.NotNull(resultGraph);
            Assert.Equal(resultNodes.Count, resultGraph.Nodes.Count);
            Assert.Equal(resultRelations.Count, resultGraph.Relations.Count);
            
            Assert.Equal(
                ((TransformerNodeViewModel)resultNodes[2]).FirstWinding,
                ((TransformerNodeViewModel)resultGraph.Nodes[2]).FirstWinding
            );

            Assert.Equal(
                ((TransformerNodeViewModel)resultNodes[5]).FirstWinding,
                ((TransformerNodeViewModel)resultGraph.Nodes[5]).FirstWinding
            );
        }


    }
}
