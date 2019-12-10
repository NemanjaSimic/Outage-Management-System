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
					Console.WriteLine("Element is RegularNode " + newElement.Id.ToString() + " and parent is Node " + parentElement.Id.ToString() + "...");

					newEdge = TopologyElementFactory.CreateOrdinaryEdge(parentElement.Id, newElement.Id);
					parentSecondEnd.Add(newEdge.Id);

					newElement.FirstEnd = newEdge.Id;
					((Node)newElement).Parent = parentElement.Id;
				}
				else if (newElement is RegularNode && parentElement is Edge)
				{
					Console.WriteLine("Element is RegularNode " + newElement.Id.ToString() + " and parent is Edge " + parentElement.Id.ToString() +"...");

					parentSecondEnd.Add(newElement.Id);
					newElement.FirstEnd = parentElement.Id;
					((Node)newElement).Parent = parentElement.FirstEnd;
				}
				else if (newElement is Edge)
				{
					Console.WriteLine("Element is Edge " + newElement.Id.ToString() + " and parent " + parentElement.Id.ToString() + "...");
					parentSecondEnd.Add(newElement.Id);
					newElement.FirstEnd = parentElement.Id;
				}

				newElement.SecondEnd.AddRange(CreateConnectivity(newElement));
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
