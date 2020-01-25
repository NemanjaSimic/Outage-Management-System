using Outage.Common.ServiceProxies.PubSub;
using Outage.Common;
using System;

namespace TopologyServiceClientMock
{
	class Program
    {
        static void Main(string[] args)
        {
			Subscriber sub = new Subscriber();
			try
			{
				using (var proxy = new TopologyServiceProxy("TopologyServiceEndpoint"))
				{
					var ui = proxy.GetTopology();
					sub.PrintUI(ui);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			SubscriberProxy subProxy = new SubscriberProxy(sub, EndpointNames.SubscriberEndpoint);
			subProxy.Subscribe(Topic.TOPOLOGY);
			Console.ReadLine();
        }

		//static void PrintUI(UIModel topology)
		//{
		//	if (topology.Nodes.Count > 0)
		//	{
		//		Print(topology.Nodes[topology.FirstNode], topology);
		//	}
		//}

		//static void Print(UINode parent, UIModel topology)
		//{
		//	var connectedElements = topology.GetRelatedElements(parent.Gid);
		//	if (connectedElements != null)
		//	{
		//		foreach (var connectedElement in connectedElements)
		//		{
		//			Console.WriteLine($"{parent.Type} with gid {parent.Gid.ToString("X")} connected to {topology.Nodes[connectedElement].Type} with gid {topology.Nodes[connectedElement].Gid.ToString("X")}");
		//			Print(topology.Nodes[connectedElement], topology);
		//		}
		//	}
		//}
	}
}
