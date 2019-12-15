using CECommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopologyElementsFuntions
{
	public class TopologyConnectivity
	{
		private readonly TopologyHelper topologyHelper = new TopologyHelper();
		private readonly TopologyElementFactory topologyElementFactory = new TopologyElementFactory();

		private List<Field> fields = new List<Field>();
		private HashSet<long> visited = new HashSet<long>();
		private Stack<TopologyElement> stack = new Stack<TopologyElement>();

		public List<TopologyElement> MakeAllTopologies()
		{
			List<TopologyElement> firstNodes = new List<TopologyElement>();
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Restart();
			List<long> energySources = topologyHelper.GetAllEnergySources();
			stopwatch.Stop();
			Console.WriteLine("GetAllEnergyResources for " + stopwatch.Elapsed.ToString());
			foreach (var energySourceGid in energySources)
			{
				stopwatch.Restart();
				firstNodes.Add(CreateTopologyFromFirstElement(energySourceGid));
				stopwatch.Stop();
				Console.WriteLine("Algorithm for " + stopwatch.Elapsed.ToString());
			}
			return firstNodes;
		}

		private TopologyElement CreateTopologyFromFirstElement(long elementGid)
		{
			RegularNode energySourceNode = new RegularNode(elementGid, TopologyStatus.Regular)
			{
				FirstEnd = null,
				Parent = null
			};

			visited.Add(elementGid);
			stack.Push(energySourceNode);

			while (stack.Count > 0)
			{
				var current = stack.Pop();
				if (!visited.Contains(current.Id))
				{
					visited.Add(current.Id);
				}

				var connectedElements = CheckIgnorable(current.Id);
				foreach (var element in connectedElements)
				{
					visited.Add(element);
					var newNode = CreateConnectivity(current, element);
					stack.Push(newNode);
				}
			}
			return energySourceNode;
		}

		List<long> CheckIgnorable(long gid)
		{
			var list = topologyHelper.GetAllReferencedElements(gid).Where(e => !visited.Contains(e)).ToList();
			List<long> elements = new List<long>();
			foreach (var element in list)
			{
				if (topologyHelper.GetElementTopologyStatus(element) == TopologyStatus.Ignorable)
				{
					visited.Add(element);
					elements.AddRange(CheckIgnorable(element));
				}
				else
				{
					elements.Add(element);
				}
			}

			return elements;
		}

		public void PrintTopology(TopologyElement firstElement)
		{
			foreach (var connectedElement in firstElement.SecondEnd)
			{
				Console.WriteLine($"{topologyHelper.GetDMSTypeOfTopologyElement(firstElement.Id)} with gid {firstElement.Id.ToString("X")} connected to {topologyHelper.GetDMSTypeOfTopologyElement(connectedElement.Id)} with gid {connectedElement.Id.ToString("X")}");
				PrintTopology(connectedElement);
			}
		}

		public TopologyElement CreateConnectivity(TopologyElement parentElement, long element)
		{
			List<TopologyElement> parentSecondEnd = new List<TopologyElement>();
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Restart();
			TopologyElement newElement = topologyElementFactory.CreateTopologyElement(element);			
			stopwatch.Stop();
			Console.WriteLine("Created new element for " + stopwatch.Elapsed.ToString());


			if (newElement is RegularNode && parentElement is RegularNode)
			{
				//Console.WriteLine("Element is RegularNode " + newElement.Id.ToString("X") + " and parent is Node " + parentElement.Id.ToString("X") + "...");
				stopwatch.Restart();
				parentSecondEnd.Add(ConnectTwoNodes(newElement, parentElement));
				stopwatch.Stop();
				Console.WriteLine($"Two Nodes: {newElement.Id.ToString("X")} and {parentElement.Id.ToString("X")} for {stopwatch.Elapsed.ToString()}");

			}
			else if (newElement is RegularNode && parentElement is Edge)
			{
				//Console.WriteLine("Element is RegularNode " + newElement.Id.ToString("X") + " and parent is Edge " + parentElement.Id.ToString("X") + "...");
				stopwatch.Restart();
				parentSecondEnd.Add(ConnectNodeWithEdge(newElement, parentElement));
				stopwatch.Stop();
				Console.WriteLine($"Node and Edge: {newElement.Id.ToString("X")} and {parentElement.Id.ToString("X")} for {stopwatch.Elapsed.ToString()}");
			}
			else if (newElement is Edge)
			{
				//Console.WriteLine("Element is Edge " + newElement.Id.ToString("X") + " and parent " + parentElement.Id.ToString("X") + "...");			
				stopwatch.Restart();
				parentSecondEnd.Add(ConnectEdgeWithTopologyElement(newElement, parentElement));
				stopwatch.Stop();
				Console.WriteLine($"New Edge: {newElement.Id.ToString("X")} and {parentElement.Id.ToString("X")} for {stopwatch.Elapsed.ToString()}");
			}

			parentElement.SecondEnd.AddRange(parentSecondEnd);
			return newElement;
		}

		#region MakingConnectionIncludingFields
		private Field GetField(long memberGid)
		{
			Field field = null;
			for (int i = 0; i < fields.Count; i++)
			{
				if (fields[i].Members.Where(e => e.Id == memberGid).ToList().Count > 0)
				{
					return fields[i];
				}
			}
			return field;
		}
		private TopologyElement ConnectEdgeWithTopologyElement(TopologyElement edge, TopologyElement topologyElement)
		{
			if (topologyHelper.GetElementTopologyStatus(topologyElement.Id) == TopologyStatus.Field)
			{
				Field field = GetField(topologyElement.Id);
				field.SecondEnd.Add(edge);
				edge.FirstEnd = field;
			}
			else
			{
				edge.FirstEnd = topologyElement;
			}
			return edge;
		}
		private TopologyElement ConnectNodeWithEdge(TopologyElement node, TopologyElement Edge)
		{
			TopologyElement parentSecondEnd;
			if (topologyHelper.GetElementTopologyStatus(node.Id) == TopologyStatus.Field)
			{
				Console.WriteLine("Creating field...");
				Field field = new Field(node)
				{
					FirstEnd = Edge,
					Parent = Edge.FirstEnd
				};
				node.FirstEnd = Edge;
				((Node)node).Parent = Edge.FirstEnd;

				fields.Add(field);
				parentSecondEnd = field;
			}
			else
			{
				parentSecondEnd = node;
				node.FirstEnd = Edge;
				((Node)node).Parent = Edge.FirstEnd;
			}
			return parentSecondEnd;
		}
		private TopologyElement ConnectTwoNodes(TopologyElement newElement, TopologyElement parent)
		{
			bool newElementIsField = topologyHelper.GetElementTopologyStatus(newElement.Id) == TopologyStatus.Field;
			bool parentElementIsField = topologyHelper.GetElementTopologyStatus(parent.Id) == TopologyStatus.Field;
			Node firstNode = parent as Node;
			Node secondNode = newElement as Node;
			Edge newEdge;

			if (newElementIsField && !parentElementIsField)
			{
				Console.WriteLine("Creating field...");
				Field field = new Field(newElement);
				Node fieldNode = field as Node;

				newEdge = MakeEdgeBetweenNodes(firstNode, fieldNode);
				fields.Add(field);
			}
			else if (newElementIsField && parentElementIsField)
			{
				newEdge = MakeEdgeBetweenNodes(firstNode, secondNode);

				Field field = GetField(parent.Id);
				field.Members.Add(newElement);
			}
			else if (!newElementIsField && parentElementIsField)
			{
				Field field = GetField(parent.Id);
				Node fieldNode = field as Node;

				newEdge = MakeEdgeBetweenNodes(fieldNode, secondNode);
				field.SecondEnd.Add(newEdge);
			}
			else
			{
				newEdge = MakeEdgeBetweenNodes(firstNode, secondNode);				
			}
			return newEdge;
		}
		private Edge MakeEdgeBetweenNodes(Node firstElement, Node secondElement)
		{
			Edge newEdge = topologyElementFactory.CreateOrdinaryEdge(firstElement, secondElement);
			secondElement.FirstEnd = newEdge;
			secondElement.Parent = firstElement;
			return newEdge;
		}
		#endregion


	}
}
