using System;
using System.Collections.Generic;
using OMS.Web.Adapter.Mock.Client;
using OMS.Web.UI.Models;

namespace OMS.Web.Adapter.Mock
{
    class Program
    {
        static void Main(string[] args)
        {
            ClientProxy proxy = new ClientProxy("net.tcp://localhost:9990/WebService");

            // mock data
            List<Node> nodes = new List<Node>
            {
                new Node { Id = "1", Name = "Energy Source 1", Type="ES", Value = 220.0f },
                new Node { Id = "2", Name = "Power Transformer 1", Type="PT", Value = 110.0f },
                new Node { Id = "3", Name = "Power Transformer 2", Type="PT", Value = 110.0f},
                new Node { Id = "4", Name = "Breaker 1", Type="BR", Value = 110.0f },
                new Node { Id = "5", Name = "Breaker 2", Type="BR", Value = 110.0f }
            };

            List<Relation> relations = new List<Relation>
            {
                new Relation { SourceNodeId = "1", TargetNodeId = "2", IsActive = true, IsAclLine = true },
                new Relation { SourceNodeId = "1", TargetNodeId = "3", IsActive = true, IsAclLine = true },
                new Relation { SourceNodeId = "2", TargetNodeId = "4", IsActive = true, IsAclLine = false },
                new Relation { SourceNodeId = "3", TargetNodeId = "5", IsActive = false, IsAclLine = false }
            };

            try
            {
                proxy.UpdateGraph(nodes, relations);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception occured during ClientProxy.UpdateGraph(): {e.Message}");
            }

            Console.ReadLine();
        }
    }
}
