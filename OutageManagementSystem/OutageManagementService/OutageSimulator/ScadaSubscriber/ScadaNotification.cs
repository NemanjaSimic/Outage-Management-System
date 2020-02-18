using OMS.OutageSimulator.BindingModels;
using Outage.Common;
using Outage.Common.PubSub;
using Outage.Common.PubSub.SCADADataContract;
using Outage.Common.ServiceContracts.PubSub;
using Outage.Common.ServiceContracts.SCADA;
using Outage.Common.ServiceProxies;
using Outage.Common.ServiceProxies.SCADA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.OutageSimulator.ScadaSubscriber
{
    public class ScadaNotification : ISubscriberCallback
    {
        private ILogger logger;

        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private ProxyFactory proxyFactory;
        private string subscriberName;

        private long outageElementId;
        private Dictionary<long, DiscreteModbusData> optimumIsolationPointsModbusData;
        private Dictionary<long, DiscreteModbusData> defaultIsolationPointsModbusData;


        public ScadaNotification(string subscriberName, ActiveOutageBindingModel outage)
        {
            this.subscriberName = subscriberName;
            this.outageElementId = outage.OutageElement.GID;
            this.proxyFactory = new ProxyFactory();

            optimumIsolationPointsModbusData = new Dictionary<long, DiscreteModbusData>(outage.OptimumIsolationPoints.Count);
            defaultIsolationPointsModbusData = new Dictionary<long, DiscreteModbusData>(outage.DefaultIsolationPoints.Count);

            GetInegrityUpdateFromScada(outage);
        }

        public string GetSubscriberName()
        {
            return subscriberName;
        }

        public void Notify(IPublishableMessage message)
        {
            if(message is MultipleDiscreteValueSCADAMessage multipleDiscreteValueSCADAMessage)
            {
                //UPDATE DATA

                foreach (long gid in optimumIsolationPointsModbusData.Keys)
                {
                    if (multipleDiscreteValueSCADAMessage.Data.ContainsKey(gid))
                    {
                        if(multipleDiscreteValueSCADAMessage.Data[gid].Value != optimumIsolationPointsModbusData[gid].Value)
                        {
                            optimumIsolationPointsModbusData[gid] = multipleDiscreteValueSCADAMessage.Data[gid];
                        }
                    }
                }

                foreach (long gid in defaultIsolationPointsModbusData.Keys)
                {
                    if (multipleDiscreteValueSCADAMessage.Data.ContainsKey(gid))
                    {
                        if (multipleDiscreteValueSCADAMessage.Data[gid].Value != defaultIsolationPointsModbusData[gid].Value)
                        {
                            defaultIsolationPointsModbusData[gid] = multipleDiscreteValueSCADAMessage.Data[gid];
                        }
                    }
                }

                //COMMAND IF TRUE

                foreach (long gid in optimumIsolationPointsModbusData.Keys)
                {
                    if(optimumIsolationPointsModbusData[gid].Value != 0)
                    {
                        OpenDefaultIsolationPoints();
                        break;
                    }
                }
            }
            else
            {
                string errorMessage = "SCADA returned wrong value for in SCADAPublication. MultipleDiscreteValueSCADAMessage excepted.";
                throw new Exception(errorMessage);
            }
        }

        private void GetInegrityUpdateFromScada(ActiveOutageBindingModel outage)
        {
            using (SCADAIntegrityUpdateProxy proxy = proxyFactory.CreateProxy<SCADAIntegrityUpdateProxy, ISCADAIntegrityUpdateContract>(EndpointNames.SCADAIntegrityUpdateEndpoint))
            {
                if(proxy == null)
                {
                    string message = "GetInegrityUpdateFromScada => SCADAIntegrityUpdateProxy is null";
                    Logger.LogError(message);
                    throw new NullReferenceException(message);
                }

                SCADAPublication data = proxy.GetIntegrityUpdateForSpecificTopic(Topic.SWITCH_STATUS);
                
                if(data.Message is MultipleDiscreteValueSCADAMessage multipleDiscreteValueSCADAMessage)
                {
                    //UPDATE DATA

                    foreach(long gid in outage.OptimumIsolationPoints.Select(p => p.GID))
                    {
                        if(multipleDiscreteValueSCADAMessage.Data.ContainsKey(gid))
                        {
                            //TODO: MEAS TO ELEMENT MAPPING
                            optimumIsolationPointsModbusData.Add(gid, multipleDiscreteValueSCADAMessage.Data[gid]);
                        }
                    }

                    foreach (long gid in outage.DefaultIsolationPoints.Select(p => p.GID))
                    {
                        if (multipleDiscreteValueSCADAMessage.Data.ContainsKey(gid))
                        {
                            //TODO: MEAS TO ELEMENT MAPPING
                            defaultIsolationPointsModbusData.Add(gid, multipleDiscreteValueSCADAMessage.Data[gid]);
                        }
                    }

                    //COMMAND IF TRUE

                    foreach (long gid in optimumIsolationPointsModbusData.Keys)
                    {
                        if (optimumIsolationPointsModbusData[gid].Value != 0)
                        {
                            OpenDefaultIsolationPoints();
                            break;
                        }
                    }
                }
                else
                {
                    string message = "SCADA returned wrong value for in SCADAPublication. MultipleDiscreteValueSCADAMessage excepted.";
                    throw new Exception(message);
                }
            }
        }

        private void OpenDefaultIsolationPoints()
        {
            using(SCADACommandProxy proxy = proxyFactory.CreateProxy<SCADACommandProxy, ISCADACommand>(EndpointNames.SCADACommandService))
            {
                if (proxy == null)
                {
                    string message = "OpenDefaultIsolationPoints => SCADACommandProxy is null";
                    Logger.LogError(message);
                    throw new NullReferenceException(message);
                }

                foreach(long gid in defaultIsolationPointsModbusData.Keys)
                {
                    if(defaultIsolationPointsModbusData[gid].Value != (ushort)DiscreteCommandingType.OPEN)
                    {
                        //TODO: COMMANDING ENUM u COMMON
                        proxy.SendDiscreteCommand(gid, (ushort)DiscreteCommandingType.OPEN);
                    }
                }
            }
        }
    }
}
