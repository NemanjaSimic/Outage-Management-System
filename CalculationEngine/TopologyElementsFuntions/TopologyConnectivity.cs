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

		public RegularNode CreateToplogy(long firstElement)
		{
			TopologyElement newElement = TopologyElementFactory.CreateTopologyElement(firstElement);
			RegularNode firstEl = newElement as RegularNode;
			foreach (var element in topologyHelper.GetAllReferencedElements(firstElement))
			{	
					List<long> newNodeSecondEnd = new List<long>();
					firstEl.Children.AddRange(CreateTopologyConnection(firstEl, element, out newNodeSecondEnd));
					firstEl.SecondEnd.AddRange(newNodeSecondEnd);			
			}
			return firstEl;
		}

		List<Node> CreateTopologyConnection(Node parentNode, long newElementGid, out List<long> mySecondEnd)
		{
			List<Node> children = new List<Node>();
			TopologyElement newElement = TopologyElementFactory.CreateTopologyElement(newElementGid);
			mySecondEnd = new List<long>();

			if (newElement is RegularNode && ((RegularNode)newElement).TopologyStatus != TopologyStatus.Ignorable)
			{
				Edge newEdge = TopologyElementFactory.CreateOrdinaryEdge(parentNode.Id, newElementGid);
				parentNode.SecondEnd.Add(newEdge.Id);
				mySecondEnd.Add(newEdge.Id);


				RegularNode newNode = newElement as RegularNode;
				newNode.Id = newElementGid;
				newNode.FirstEnd = newEdge.Id;
				newNode.Parent = parentNode.Id;

				foreach (var element in topologyHelper.GetAllReferencedElements(newElementGid))
				{
					if (element != parentNode.Id)
					{
						List<long> newNodeSecondEnd = new List<long>();
						newNode.Children.AddRange(CreateTopologyConnection(newNode, element, out newNodeSecondEnd));
						newNode.SecondEnd.AddRange(newNodeSecondEnd);
					}
				}
				children.Add(newNode);
				children.AddRange(newNode.Children);
			}
			else if (newElement is Edge)
			{
				Edge newEdge = newElement as Edge;
				newEdge.Id = newElementGid;
				newEdge.FirstEnd = parentNode.Id;
				newEdge.SecondEnd = topologyHelper.GetAllReferencedElements(newElementGid).FirstOrDefault();
				mySecondEnd.Add(newEdge.Id);
				CreateTopologyConnection(parentNode, newEdge.SecondEnd, out List<long> secondEnd);
			}
			else
			{
				foreach (var element in topologyHelper.GetAllReferencedElements(newElementGid))
				{
					if (element != parentNode.Id)
					{
						List<long> newNodeSecondEnd = new List<long>();
						CreateTopologyConnection(parentNode, element, out newNodeSecondEnd);
						mySecondEnd.AddRange(newNodeSecondEnd);
					}
				}
			}


			return children;
		}

		public RegularNode Topology(long energySourceGid)
		{
			RegularNode es = new RegularNode(energySourceGid, TopologyStatus.Regular);
			es.FirstEnd = null;
			es.SecondEnd.AddRange(Create(es));
			return es;
		}

		public List<long> Create(TopologyElement parentElement)
		{
			List<long> parentSecondEnd = new List<long>();
			Edge newEdge;
			List<long> connElements = GetAllConnectedElements(parentElement);
			foreach (var element in connElements)
			{
				
				TopologyElement newElement = TopologyElementFactory.CreateTopologyElement(element);
				if (newElement is RegularNode && parentElement is Node)
				{
					Console.WriteLine("Element is RegularNode " + newElement.Id.ToString() + " and parent is Node " + parentElement.Id.ToString() + "...");

					RegularNode newNode = newElement as RegularNode;

					newEdge = TopologyElementFactory.CreateOrdinaryEdge(parentElement.Id, newNode.Id);
					parentSecondEnd.Add(newEdge.Id);
					newNode.FirstEnd = newEdge.Id;
					newNode.Parent = parentElement.Id;

					newNode.SecondEnd.AddRange(Create(newNode));
				}
				else if (newElement is RegularNode && parentElement is Edge)
				{
					Console.WriteLine("Element is RegularNode " + newElement.Id.ToString() + " and parent is Edge " + parentElement.Id.ToString() +"...");
					RegularNode newNode = newElement as RegularNode;

					newNode.FirstEnd = parentElement.Id;
					newNode.Parent = parentElement.FirstEnd;
					parentSecondEnd.Add(newNode.Id);
					newNode.SecondEnd.AddRange(Create(newNode));
				}
				else if (newElement is Edge && parentElement is RegularNode)
				{
					Console.WriteLine("Element is Edge " + newElement.Id.ToString() + " and parent " + parentElement.Id.ToString() + "...");
					newEdge = newElement as Edge;
					parentSecondEnd.Add(newEdge.Id);
					newEdge.FirstEnd = parentElement.Id;
					newEdge.SecondEnd = GetAllConnectedElements(newEdge).FirstOrDefault();
					RegularNode newNode = TopologyElementFactory.CreateTopologyElement(newEdge.SecondEnd) as RegularNode;
					newNode.FirstEnd = newEdge.Id;
					newNode.Parent = newEdge.FirstEnd;
					Create(newNode);
				}
				else
				{
					Console.WriteLine("NULL");
				}
			}

			return parentSecondEnd;
		}

		private List<long> GetAllConnectedElements(TopologyElement element)
		{
			if (element is Node)
			{
				return topologyHelper.GetAllReferencedElements(element.Id).FindAll(e => e != ((Node)element).Parent && e != element.FirstEnd);
			}
			else
			{
				return topologyHelper.GetAllReferencedElements(element.Id).FindAll(e => e != element.FirstEnd);
			}
		}
	}
}
