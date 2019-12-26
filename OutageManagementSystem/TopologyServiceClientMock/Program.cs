using CECommon.Model.UI;
using System;

namespace TopologyServiceClientMock
{
	class Program
    {
        static void Main(string[] args)
        {
            using (var proxy = new TopologyServiceProxy("TopologyServiceEndpoint"))
            {
				var ui = proxy.GetTopology();
				PrintUI(ui);
				Console.ReadLine();
            }
        }

		static void PrintUI(UIModel topology)
		{
			Print(topology.Nodes[topology.FirstNode], topology);
		}

		static void Print(UINode parent, UIModel topology)
		{
			var connectedElements = topology.GetRelatedElements(parent.Gid);
			if (connectedElements != null)
			{
				foreach (var connectedElement in connectedElements)
				{
					Console.WriteLine($"{parent.Type} with gid {parent.Gid.ToString("X")} connected to {topology.Nodes[connectedElement].Type} with gid {topology.Nodes[connectedElement].Gid.ToString("X")}");
					Print(topology.Nodes[connectedElement], topology);
				}
			}
		}
	}
}
