using Outage.Common;
using Outage.Common.PubSub;
using Outage.Common.PubSub.CalculationEngineDataContract;
using Outage.Common.ServiceContracts.PubSub;
using Outage.Common.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace TopologyServiceClientMock
{
	[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
	class Subscriber : ISubscriberCallback
    {
        public string GetSubscriberName()
        {
            return "Test client mock";
        }

        public void Notify(IPublishableMessage message)
        {
			TopologyForUIMessage model = message as TopologyForUIMessage;
			PrintUI(model.UIModel);
		}

		public void PrintUI(UIModel topology)
		{
			if (topology.Nodes.Count > 0)
			{
				Print(topology.Nodes[topology.FirstNode], topology);
			}
		}

		void Print(UINode parent, UIModel topology)
		{
			var connectedElements = topology.GetRelatedElements(parent.Gid);
			if (connectedElements != null)
			{
				foreach (var connectedElement in connectedElements)
				{
					Console.WriteLine($"{parent.DMSType} with gid {parent.Gid.ToString("X")} connected to {topology.Nodes[connectedElement].DMSType} with gid {topology.Nodes[connectedElement].Gid.ToString("X")}");
					Console.WriteLine($"NominalVoltage: {parent.NominalVoltage}; MeasurementType: {parent.MeasurementType}; MeasurementValue:{parent.Measurement}; IsActive: {parent.IsActive}");
					Print(topology.Nodes[connectedElement], topology);
				}
			}
		}
	}
}
