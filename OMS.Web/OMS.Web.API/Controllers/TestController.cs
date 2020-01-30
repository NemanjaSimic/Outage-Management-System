namespace OMS.Web.API.Controllers
{
    using OMS.Web.UI.Models.ViewModels;
    using System.Collections.Generic;
    using System.Web.Http;

    public class TestController : ApiController
    {
        private readonly List<Node> testNodes = new List<Node>
        {
            new Node { Id = "1", DMSType="ENERGYSOURCE" },
            new Node { Id = "2", DMSType="LOADBREAKSWITCH" },
            new TransformerNode { Id = "4", DMSType="POWERTRANSFORMER" },
            new Node { Id = "6", DMSType="LOADBREAKSWITCH" },
            new Node { Id = "7", DMSType="LOADBREAKSWITCH" },
            new TransformerNode { Id = "9", DMSType="POWERTRANSFORMER" },
            new Node { Id = "11", DMSType="LOADBREAKSWITCH" }
        };

        private readonly List<Relation> testRelations = new List<Relation>
        {
            new Relation { SourceNodeId = "1", TargetNodeId = "2" },
            new Relation { SourceNodeId = "2", TargetNodeId = "4" },
            new Relation { SourceNodeId = "4", TargetNodeId = "6" },
            new Relation { SourceNodeId = "1", TargetNodeId = "7" },
            new Relation { SourceNodeId = "7", TargetNodeId = "9" },
            new Relation { SourceNodeId = "9", TargetNodeId = "11" }
        };

        [HttpGet]
        public IHttpActionResult Get()
        {
            // set windings
            ((TransformerNode)testNodes[2]).FirstWinding = new Node { Id = "3", DMSType = "TRANSFORMERWINDING" };
            ((TransformerNode)testNodes[2]).SecondWinding = new Node { Id = "5", DMSType = "TRANSFORMERWINDING" };

            ((TransformerNode)testNodes[5]).FirstWinding = new Node { Id = "8", DMSType = "TRANSFORMERWINDING" };
            ((TransformerNode)testNodes[5]).SecondWinding = new Node { Id = "10", DMSType = "TRANSFORMERWINDING" };

            OmsGraph graph = new OmsGraph
            {
                Nodes = testNodes,
                Relations = testRelations
            };

            return Ok(graph);
        }
    }
}
