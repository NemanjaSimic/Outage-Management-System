namespace OMS.Web.API.Controllers
{
    using Microsoft.AspNet.SignalR;
    using OMS.Web.API.Hubs;
    using OMS.Web.UI.Models;
    using OMS.Web.UI.Models.ViewModels;
    using System;
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
            new NodeViewModel { Id = "11", DMSType="LOADBREAKSWITCH" },
            new NodeViewModel { Id = "12", DMSType="ACLINESEGMENT" },
            new NodeViewModel { Id = "13", DMSType="ACLINESEGMENT" },
            new NodeViewModel { Id = "14", DMSType="FUSE" },
            new NodeViewModel { Id = "15", DMSType="FUSE" },
            new NodeViewModel { Id = "16", DMSType="ENERGYCONSUMER" },
            new NodeViewModel { Id = "17", DMSType="ENERGYCONSUMER" }
        };

        private readonly List<RelationViewModel> testRelations = new List<RelationViewModel>
        {
            new RelationViewModel { SourceNodeId = "1", TargetNodeId = "2" },
            new RelationViewModel { SourceNodeId = "2", TargetNodeId = "4" },
            new RelationViewModel { SourceNodeId = "4", TargetNodeId = "6" },
            new RelationViewModel { SourceNodeId = "1", TargetNodeId = "7" },
            new RelationViewModel { SourceNodeId = "7", TargetNodeId = "9" },
            new RelationViewModel { SourceNodeId = "9", TargetNodeId = "11" },

            new RelationViewModel { SourceNodeId = "6", TargetNodeId = "12" },
            new RelationViewModel { SourceNodeId = "6", TargetNodeId = "13" },
            new RelationViewModel { SourceNodeId = "12", TargetNodeId = "14" },
            new RelationViewModel { SourceNodeId = "13", TargetNodeId = "15" },
            new RelationViewModel { SourceNodeId = "14", TargetNodeId = "16" },
            new RelationViewModel { SourceNodeId = "15", TargetNodeId = "17" },
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

        [HttpGet]
        [Route("api/test/initialoutage")]
        public IHttpActionResult GetInitialOutage()
        {
            ActiveOutageViewModel initialActiveOutage = new ActiveOutageViewModel
            {
                Id = 111,
                DefaultIsolationPoints = new List<long> { 6 },
                State = OutageLifecycleState.Created,
                ReportedAt = DateTime.Now
            };

            var outageHubContext = GetOutageHubContext();
            outageHubContext.Clients.All.activeOutageUpdate(initialActiveOutage);

            return Ok();
        }

        [HttpPost]
        [Route("api/test/isolate/{id}")]
        public IHttpActionResult IsolateOutageTest([FromUri]long id)
        {
            ActiveOutageViewModel initialActiveOutage = new ActiveOutageViewModel
            {
                Id = id,
                DefaultIsolationPoints = new List<long> { 6 },
                ElementId = 12, // ACLINE segment
                // breaker iznad i fuse ispod (stavio sam elemente okolo aclinesegmenta), ali front ce raditi sa bilo kojim vasim id-evima
                // mada ovo nije ni bitno toliko za front ?
                OptimalIsolationPoints = new List<long> { 6, 14 }, 
                State = OutageLifecycleState.Isolated,
                ReportedAt = DateTime.Now // ovo se ne menja al test
            };

            var outageHubContext = GetOutageHubContext();
            outageHubContext.Clients.All.activeOutageUpdate(initialActiveOutage);

            return Ok();
        }

        [HttpPost]
        [Route("api/test/sendcrew/{id}")]
        public IHttpActionResult SendRepairCrew([FromUri]long id)
        {
            ActiveOutageViewModel initialActiveOutage = new ActiveOutageViewModel
            {
                Id = id,
                DefaultIsolationPoints = new List<long> { 6 },
                ElementId = 12, // ACLINE segment
                // breaker iznad i fuse ispod (stavio sam elemente okolo aclinesegmenta), ali front ce raditi sa bilo kojim vasim id-evima
                // mada ovo nije ni bitno toliko za front ?
                OptimalIsolationPoints = new List<long> { 6, 14 },
                State = OutageLifecycleState.Isolated,
                ReportedAt = DateTime.Now, // ovo nije isto, al test je
                RepairedAt = DateTime.Now
            };

            var outageHubContext = GetOutageHubContext();
            outageHubContext.Clients.All.activeOutageUpdate(initialActiveOutage);

            return Ok();
        }

        [HttpPost]
        [Route("api/test/resolve/{id}")]
        public IHttpActionResult ResolveOutage([FromUri]long id)
        {
            // ovde se prebacuje u archived stanje
            return Ok();
        }

        [HttpPost]
        [Route("api/test/validate/{id}")]
        public IHttpActionResult ValidateOutage([FromUri]long id)
        {
            ActiveOutageViewModel initialActiveOutage = new ActiveOutageViewModel
            {
                Id = id,
                DefaultIsolationPoints = new List<long> { 6 },
                ElementId = 12, // ACLINE segment
                // breaker iznad i fuse ispod (stavio sam elemente okolo aclinesegmenta), ali front ce raditi sa bilo kojim vasim id-evima
                // mada ovo nije ni bitno toliko za front ?
                OptimalIsolationPoints = new List<long> { 6, 14 },
                State = OutageLifecycleState.Isolated,
                ReportedAt = DateTime.Now, // ovo nije isto, al test je
                RepairedAt = DateTime.Now,
                IsValidated = true  // validiramo ga
            };

            var outageHubContext = GetOutageHubContext();
            outageHubContext.Clients.All.activeOutageUpdate(initialActiveOutage);

            return Ok();
        }

        private IHubContext GetOutageHubContext() => GlobalHost.ConnectionManager.GetHubContext<OutageHub>();
    }
}
