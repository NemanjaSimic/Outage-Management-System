using Outage.Common;
using Outage.Common.PubSub;
using Outage.Common.PubSub.EmailDataContract;
using Outage.Common.ServiceContracts.OMS;
using Outage.Common.ServiceContracts.PubSub;
using Outage.Common.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Timers;

namespace OutageManagementService.Calling
{
    //[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
   // public class CallingService : ICallingContract,ISubscriberCallback
   // {
   //     private ILogger logger;
   //     protected ILogger Logger
   //     {
   //         get { return logger ?? (logger = LoggerWrapper.Instance); }
   //     }

   //     public string SubscriberName { get; set; }
   //     public OutageModel outageModel { get; set; }
   //     public Timer timer = null;
   //     public CallingService(string SubscriberName)
   //     {
   //         this.SubscriberName = SubscriberName;
   //     }
   //     public string GetSubscriberName()
   //     {
   //         return this.SubscriberName;
   //     }

   //     public void Notify(IPublishableMessage message)
   //     {
   //         if(message is EmailToOutageMessage emailMsg)
   //         {
   //             this.outageModel.EmailMsg.Enqueue(emailMsg.Gid);
   //             SetTimer();
   //         }
   //     }
        
   //     private void SetTimer()
   //     {
   //         if(timer == null)
   //         {
   //             timer = new Timer(Int32.Parse(ConfigurationManager.AppSettings["TimerInterval"]));
   //             timer.Elapsed += PackCalls;
   //             timer.AutoReset = false;
   //             timer.Enabled = true;
   //         }
   //     }

   //     private void PackCalls(object sender, ElapsedEventArgs e)
   //     {
   //         this.outageModel.CalledOutages.Clear();
   //         while (this.outageModel.EmailMsg.TryDequeue(out long call)) {

   //             if (this.outageModel.topology.Nodes.TryGetValue(call, out UINode uiNode))
   //             {
   //                 if (!uiNode.IsActive) this.outageModel.CalledOutages.Add(call);
   //             }
   //         }
   //         Tracing();
   //     }
   //     private void Tracing()
   //     {

   //         List<long> outage = new List<long>();
   //         foreach (var call in this.outageModel.CalledOutages)
   //         {
   //             // nece build da prodje jer ti je ovde FirstEnd tipa long, a nije struktura
   //             // pa ne postoji .Id polje kod njega
   //             //outage.Add(this.outageModel.topologyModel.TopologyElements[call].FirstEnd.Id);
   //         }

   //         foreach (var item in outage)
   //         {
   //             //Izbaci ako postoji prijavljen roditelj
   //         }

          
   //             //Provera da li je prijavljen outage ustv na tom mestu ili mozda se desio negde kod roditelja
            


   //         foreach (var item in outage)
   //         {
   //             ReportMalfunction(item);
   //         }
   //         /*//Find outage from called user gid
			//Dictionary<long, int> numbOfNodes = new Dictionary<long, int>();
			//Dictionary<long, int> temp = new Dictionary<long, int>();
			//var firstNode = this.outageModel.topology.FirstNode;
			//foreach (var call in this.outageModel.calls)
			//{

			//	foreach (var nodes in this.outageModel.topology.Relations)
			//	{
			//		if (nodes.Value.Contains(call)) outage.Add(nodes.Key);
			//	}
			//}

			//bool Done = false;
			//while (!Done) {
			//	foreach (var item in outage)
			//	{
			//		foreach (var nodes in this.outageModel.topology.Relations)
			//		{
			//			if (nodes.Value.Contains(item))
			//			{
			//				if (numbOfNodes.ContainsKey(item)) numbOfNodes.Add(item, 1);
			//				else numbOfNodes[item]++;
			//			}
			//		}
			//	}
			//	int i = 0;
			//	Done = true;
			//	foreach (var item in numbOfNodes)
			//	{
			//		if(item.Value > 2)
			//		{
			//			Done = false;
			//			//Izabaci nodove koji su ispod tj 2+ ce se zameniti sa nodom roditeljem
			//			foreach (var node in this.outageModel.topology.Relations[item.Key])
			//			{
			//				if (outage.Contains(node)) outage.Remove(node);
			//			}
			//			outage.Add(item.Key);
			//		}
			//	}


			//}*/
   //         timer = null;

   //     }
   //     public void ReportMalfunction(long consumerGid)
   //     {
   //         //TODO: Logic
   //         Logger.LogInfo($"Malfunction reported by consumer {consumerGid}.");
   //     }
   // }
}
