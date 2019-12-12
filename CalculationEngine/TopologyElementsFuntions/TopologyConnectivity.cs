using CECommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopologyElementsFuntions
{
	public class TopologyConnectivity
	{
		private readonly TopologyHelper topologyHelper = new TopologyHelper();

		private Dictionary<long, long> topologyFirstEnd = new Dictionary<long, long>();
		private Dictionary<long, TopologyElement> topologyElements = new Dictionary<long, TopologyElement>();

		public List<long> CreateTopology()
		{
			List<long> energySources = topologyHelper.GetAllEnergySources();
			
			foreach (var energySourceGid in energySources)
			{
				RegularNode energySourceNode = new RegularNode(energySourceGid, TopologyStatus.Regular)
				{
					FirstEnd = null,
					Parent = null
				};
				energySourceNode.SecondEnd.AddRange(CreateConnectivity(energySourceNode));
				topologyElements.Add(energySourceNode.Id, energySourceNode);
			}
			return energySources;
		}

		public void PrintTopology(long firstElement)
		{
			TopologyElement elementNode = GetTopologyElement(firstElement);

			foreach (var connectedElement in elementNode.SecondEnd)
			{
				TopologyElement connectedElementNode = GetTopologyElement(connectedElement);
				Console.WriteLine($"{topologyHelper.GetDMSTypeOfTopologyElement(firstElement)} with gid {elementNode.Id.ToString("X")} connected to {topologyHelper.GetDMSTypeOfTopologyElement(connectedElement)} with gid {connectedElementNode.Id.ToString("X")}");
				PrintTopology(connectedElement);
			}
		}

		private TopologyElement GetTopologyElement(long gid)
		{
			return topologyElements[gid];
		}

		public List<long> CreateConnectivity(TopologyElement parentElement)
		{
			List<long> parentSecondEnd = new List<long>();
			List<long> connectedElements = GetAllConnectedElements(parentElement);

			foreach (var element in connectedElements)
			{
				TopologyElement newElement = TopologyElementFactory.CreateTopologyElement(element);

				if (newElement is RegularNode && parentElement is RegularNode)
				{
					//Console.WriteLine("Element is RegularNode " + newElement.Id.ToString("X") + " and parent is Node " + parentElement.Id.ToString("X") + "...");
					parentSecondEnd.Add(ConnectTwoNodes(newElement, parentElement));
				}
				else if (newElement is RegularNode && parentElement is Edge)
				{
					//Console.WriteLine("Element is RegularNode " + newElement.Id.ToString("X") + " and parent is Edge " + parentElement.Id.ToString("X") + "...");
					parentSecondEnd.Add(ConnectNodeWithEdge(newElement, parentElement));
				}
				else if (newElement is Edge)
				{
					//Console.WriteLine("Element is Edge " + newElement.Id.ToString("X") + " and parent " + parentElement.Id.ToString("X") + "...");			
					parentSecondEnd.Add(ConnectEdgeWithTopologyElement(newElement, parentElement));
				}

				newElement.SecondEnd.AddRange(CreateConnectivity(newElement));
				topologyElements.Add(newElement.Id, newElement);
			}
			return parentSecondEnd;
		}

        #region ReturnConnectedElementsWithoutUnnecessary
        private List<long> GetAllConnectedElements(TopologyElement element)
		{
			List<long> connectedElements = new List<long>();
			List<long> tempElements;

			if (element is Node)
			{
				tempElements = topologyHelper.GetAllReferencedElements(element.Id).FindAll(e => e != ((Node)element).Parent && e != element.FirstEnd && e != GetTopologyFirstEnd(element.Id));
			}
			else
			{
				tempElements = topologyHelper.GetAllReferencedElements(element.Id).FindAll(e => e != element.FirstEnd && e != GetTopologyFirstEnd(element.Id));
			}

			foreach (var connectedElement in tempElements)
			{
				if (topologyHelper.GetElementTopologyStatus(connectedElement) != TopologyStatus.Ignorable)
				{
					connectedElements.Add(connectedElement);
				}
				else
				{
					connectedElements.AddRange(GetAllConnectedElements(CreateUnnecessaryTopologyElement(connectedElement, element.Id)));
				}
			}

			return connectedElements;
		}
		private TopologyElement CreateUnnecessaryTopologyElement(long newElementGid, long parentElementGid)
		{
			TopologyElement newElement = TopologyElementFactory.CreateTopologyElement(newElementGid);
			SetTopologyFirstEnd(newElementGid, parentElementGid);

			if (newElement is Node)
			{
				((Node)newElement).Parent = parentElementGid;
			}
			else
			{
				newElement.FirstEnd = parentElementGid;
			}

			return newElement;
		}
		private long GetTopologyFirstEnd(long elementGid)
		{
			long topologyFirstEnd = 0;
			try
			{
				topologyFirstEnd = this.topologyFirstEnd[elementGid];
			}
			catch (Exception)
			{
				//log				
			}
			return topologyFirstEnd;
		}
		private void SetTopologyFirstEnd(long elementGid, long topologyFirstEnd)
		{
			try
			{
				this.topologyFirstEnd.Add(elementGid, topologyFirstEnd);
			}
			catch (Exception)
			{
				//log
			}
		}
        #endregion

        #region MakingConnectionIncludingFields
        private Field GetField(long memberGid)
		{
			Field field = null;
			foreach (var element in topologyElements)
			{
				if (element.Value is Field && ((Field)element.Value).Members.Contains(memberGid))
				{
					field = element.Value as Field;
				}
			}
			return field;
		}
		private long ConnectEdgeWithTopologyElement(TopologyElement edge, TopologyElement topologyElement)
		{
			if (topologyHelper.GetElementTopologyStatus(topologyElement.Id) == TopologyStatus.Field)
			{
				Field field = GetField(topologyElement.Id);
				field.SecondEnd.Add(edge.Id);
				edge.FirstEnd = field.Id;

				topologyElements.Remove(field.Id);
				topologyElements.Add(field.Id, field);
			}
			else
			{
				edge.FirstEnd = topologyElement.Id;
			}
			return edge.Id;
		}
		private long ConnectNodeWithEdge(TopologyElement node, TopologyElement Edge)
		{
			long parentSecondEnd;
			if (topologyHelper.GetElementTopologyStatus(node.Id) == TopologyStatus.Field)
			{
				Field field = new Field(node)
				{
					FirstEnd = Edge.Id,
					Parent = Edge.FirstEnd
				};

				node.FirstEnd = Edge.Id;
				((Node)node).Parent = Edge.FirstEnd;

				parentSecondEnd = field.Id;
				topologyElements.Add(field.Id, field);
			}
			else
			{
				parentSecondEnd = node.Id;
				node.FirstEnd = Edge.Id;
				((Node)node).Parent = Edge.FirstEnd;
			}
			return parentSecondEnd;
		}
		private long ConnectTwoNodes(TopologyElement newElement, TopologyElement parent)
		{
			Node firstNode = parent as Node;
			Node secondNode = newElement as Node;
			Edge newEdge;
			if (topologyHelper.GetElementTopologyStatus(newElement.Id) == TopologyStatus.Field 
				&& topologyHelper.GetElementTopologyStatus(parent.Id) == TopologyStatus.Regular )
			{
				Field field = new Field(newElement);
				Node fieldNode = field as Node;

				newEdge = MakeEdgeBetweenNodes(firstNode, fieldNode);
							
				topologyElements.Add(field.Id, field);
			}
			else if (topologyHelper.GetElementTopologyStatus(newElement.Id) == TopologyStatus.Field
				&& topologyHelper.GetElementTopologyStatus(parent.Id) == TopologyStatus.Field)
			{
				newEdge = MakeEdgeBetweenNodes(firstNode, secondNode);
				
				Field field = GetField(parent.Id);
				field.Members.Add(newElement.Id);
				topologyElements.Remove(field.Id);
				topologyElements.Add(field.Id, field);
			}
			else if (topologyHelper.GetElementTopologyStatus(newElement.Id) == TopologyStatus.Regular
				&& topologyHelper.GetElementTopologyStatus(parent.Id) == TopologyStatus.Field)
			{
				Field field = GetField(parent.Id);
				Node fieldNode = field as Node;

				newEdge = MakeEdgeBetweenNodes(fieldNode, secondNode);
				field.SecondEnd.Add(newEdge.Id);

				topologyElements.Remove(field.Id);
				topologyElements.Add(field.Id, field);
			}
			else
			{
				newEdge = MakeEdgeBetweenNodes(firstNode, secondNode);
			}

			topologyElements.Add(newEdge.Id, newEdge);
			return newEdge.Id;
		}
		private Edge MakeEdgeBetweenNodes(Node firstElement, Node secondElement)
		{
			Edge newEdge = TopologyElementFactory.CreateOrdinaryEdge(firstElement.Id, secondElement.Id);
			secondElement.FirstEnd = newEdge.Id;
			secondElement.Parent = firstElement.Id;

			return newEdge;
		}
        #endregion
    }
}
