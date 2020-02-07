namespace OMS.Web.API.Controllers
{
    using OMS.Web.UI.Models.ViewModels;
    using System.Collections.Generic;
    using System.Web.Http;

    public class TestController : ApiController
    {
        private readonly List<NodeViewModel> testNodes = new List<NodeViewModel>
        {
            new NodeViewModel { Id = "1", DMSType="ENERGYSOURCE" },
            new NodeViewModel { Id = "2", DMSType="LOADBREAKSWITCH" },
            new TransformerNodeViewModel { Id = "4", DMSType="POWERTRANSFORMER" },
            new NodeViewModel { Id = "6", DMSType="LOADBREAKSWITCH" },
            new NodeViewModel { Id = "7", DMSType="LOADBREAKSWITCH" },
            new TransformerNodeViewModel { Id = "9", DMSType="POWERTRANSFORMER" },
            new NodeViewModel { Id = "11", DMSType="LOADBREAKSWITCH" }
        };

        private readonly List<RelationViewModel> testRelations = new List<RelationViewModel>
        {
            new RelationViewModel { SourceNodeId = "1", TargetNodeId = "2" },
            new RelationViewModel { SourceNodeId = "2", TargetNodeId = "4" },
            new RelationViewModel { SourceNodeId = "4", TargetNodeId = "6" },
            new RelationViewModel { SourceNodeId = "1", TargetNodeId = "7" },
            new RelationViewModel { SourceNodeId = "7", TargetNodeId = "9" },
            new RelationViewModel { SourceNodeId = "9", TargetNodeId = "11" }
        };

        [HttpGet]
        public IHttpActionResult Get()
        {
            // set windings
            ((TransformerNodeViewModel)testNodes[2]).FirstWinding = new NodeViewModel { Id = "3", DMSType = "TRANSFORMERWINDING" };
            ((TransformerNodeViewModel)testNodes[2]).SecondWinding = new NodeViewModel { Id = "5", DMSType = "TRANSFORMERWINDING" };

            ((TransformerNodeViewModel)testNodes[5]).FirstWinding = new NodeViewModel { Id = "8", DMSType = "TRANSFORMERWINDING" };
            ((TransformerNodeViewModel)testNodes[5]).SecondWinding = new NodeViewModel { Id = "10", DMSType = "TRANSFORMERWINDING" };

            OmsGraphViewModel graph = new OmsGraphViewModel
            {
                Nodes = testNodes,
                Relations = testRelations
            };

            return Ok(graph);
        }
    }
}
