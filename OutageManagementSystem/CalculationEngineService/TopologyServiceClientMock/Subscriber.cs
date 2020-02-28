using Outage.Common.PubSub;
using Outage.Common.PubSub.CalculationEngineDataContract;
using Outage.Common.PubSub.SCADADataContract;
using Outage.Common.ServiceContracts.PubSub;
using Outage.Common.UI;
using System;
using System.ServiceModel;

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
			if (message is SingleAnalogValueSCADAMessage)
			{
				SingleAnalogValueSCADAMessage msg = (SingleAnalogValueSCADAMessage)message;
				Console.WriteLine($"Merenje: {msg.AnalogModbusData.MeasurementGid} {msg.AnalogModbusData.Value}");
			}
			else if(message is MultipleAnalogValueSCADAMessage)
			{
				MultipleAnalogValueSCADAMessage msg = (MultipleAnalogValueSCADAMessage)message;
				foreach (var item in msg.Data)
				{
					Console.WriteLine($"Merenje: {item.Key} {item.Value}");

				}
			}
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
			var connectedElements = topology.GetRelatedElements(parent.Id);
			if (connectedElements != null)
			{
				foreach (var connectedElement in connectedElements)
				{
					Console.WriteLine($"{parent.DMSType} with gid {parent.Id.ToString("X")} connected to {topology.Nodes[connectedElement].DMSType} with gid {topology.Nodes[connectedElement].Id.ToString("X")}");
					Console.WriteLine($"NominalVoltage: {parent.NominalVoltage}; IsActive: {parent.IsActive}");
					foreach (var measurement in parent.Measurements)
					{
						Console.WriteLine($"--Measurement-- Type: {measurement.Type}; Value: { measurement.Value};");
						
					}
					Print(topology.Nodes[connectedElement], topology);
				}
			}
		}
	}
}
