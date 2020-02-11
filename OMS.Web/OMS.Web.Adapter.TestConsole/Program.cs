namespace OMS.Web.Adapter.TestConsole
{
    using OMS.Web.Adapter.HubDispatchers;
    using OMS.Web.UI.Models.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    class Program
    {
        static void Main(string[] args)
        {
            GraphHubDispatcher dispatcher = new GraphHubDispatcher();

            List<NodeViewModel> nodes = new List<NodeViewModel>
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

            List<RelationViewModel> relations = new List<RelationViewModel>
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

            dispatcher.Connect();

            while (true)
            {
                string key = Console.ReadLine();
                if(key.ToUpper() == "S")
                    dispatcher.NotifyGraphUpdate(nodes, relations);

                if (key.ToUpper() == "Q")
                    break;
            }

        }
    }
}
