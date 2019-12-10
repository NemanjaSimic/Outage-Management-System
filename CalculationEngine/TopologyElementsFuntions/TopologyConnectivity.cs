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
		private Dictionary<long, long> TopologyFirstEnd = new Dictionary<long, long>();
		private Dictionary<long, TopologyElement> topologyElements = new Dictionary<long, TopologyElement>();

		public RegularNode CreateTopology(long energySourceGid)
		{
			RegularNode es = new RegularNode(energySourceGid, TopologyStatus.Regular);
			es.FirstEnd = null;
			es.SecondEnd.AddRange(CreateConnectivity(es));
			return es;
		}

		public List<long> CreateConnectivity(TopologyElement parentElement)
		{
			List<long> parentSecondEnd = new List<long>();
			Edge newEdge;
			List<long> connectedElements = GetAllConnectedElements(parentElement);

			foreach (var element in connectedElements)
			{
				TopologyElement newElement = TopologyElementFactory.CreateTopologyElement(element);
				
				if (newElement is RegularNode && parentElement is Node)
				{
					Console.WriteLine("Element is RegularNode " + newElement.Id.ToString("X") + " and parent is Node " + parentElement.Id.ToString("X") + "...");

					newEdge = TopologyElementFactory.CreateOrdinaryEdge(parentElement.Id, newElement.Id);
					parentSecondEnd.Add(newEdge.Id);

					newElement.FirstEnd = newEdge.Id;
					((Node)newElement).Parent = parentElement.Id;
				}
				else if (newElement is RegularNode && parentElement is Edge)
				{
					Console.WriteLine("Element is RegularNode " + newElement.Id.ToString("X") + " and parent is Edge " + parentElement.Id.ToString("X") +"...");

					parentSecondEnd.Add(newElement.Id);
					newElement.FirstEnd = parentElement.Id;
					((Node)newElement).Parent = parentElement.FirstEnd;
				}
				else if (newElement is Edge)
				{
					Console.WriteLine("Element is Edge " + newElement.Id.ToString("X") + " and parent " + parentElement.Id.ToString("X") + "...");
					parentSecondEnd.Add(newElement.Id);
					newElement.FirstEnd = parentElement.Id;
				}

				newElement.SecondEnd.AddRange(CreateConnectivity(newElement));
				topologyElements.Add(newElement.Id, newElement);
			}

			return parentSecondEnd;
		}

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
				topologyFirstEnd = TopologyFirstEnd[elementGid];
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
				TopologyFirstEnd.Add(elementGid, topologyFirstEnd);
			}
			catch (Exception)
			{
				//log
			}
		}
	}
}
