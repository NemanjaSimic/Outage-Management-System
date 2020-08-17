using OMS.Common.Cloud.Names;
using System;
using System.Collections.Generic;

namespace OMS.Common.Cloud
{
    public class ServiceDefines
    {
        #region Instance
        private static readonly object lockSync = new object();

        private static ServiceDefines instance;
        public static ServiceDefines Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockSync)
                    {
                        if (instance == null)
                        {
                            instance = new ServiceDefines();
                        }
                    }
                }

                return instance;
            }
        }
        #endregion Instance

        #region Public Properties
        private readonly Dictionary<string, ServiceType> serviceNameToServiceType;
        public Dictionary<string, ServiceType> ServiceNameToServiceType
        {
            //preventing the outside modification - getter is not called many times 
            get { return new Dictionary<string, ServiceType>(serviceNameToServiceType); }
        }

        private readonly Dictionary<string, Uri> serviceNameToServiceUri;
        public Dictionary<string, Uri> ServiceNameToServiceUri
        {
            //preventing the outside modification - getter is not called many times 
            get { return new Dictionary<string, Uri>(serviceNameToServiceUri); }
        }
        #endregion Public Properties;

        private ServiceDefines()
        {
            serviceNameToServiceType = new Dictionary<string, ServiceType>
            {
                //NMS
                { MicroserviceNames.NmsGdaService,                  ServiceType.STATELESS_SERVICE   },
                
                //SCADA
                { MicroserviceNames.ScadaModelProviderService,      ServiceType.STATEFUL_SERVICE    },
                { MicroserviceNames.ScadaFunctionExecutorService,   ServiceType.STATELESS_SERVICE   },
                { MicroserviceNames.ScadaCommandingService,         ServiceType.STATELESS_SERVICE   },
                { MicroserviceNames.ScadaAcquisitionService,        ServiceType.STATELESS_SERVICE   },
                
                //PUB_SUB
                { MicroserviceNames.PubSubService,                  ServiceType.STATEFUL_SERVICE    },
                
                //TMS
                { MicroserviceNames.TransactionManagerService,      ServiceType.STATEFUL_SERVICE    },

                //TODO: CE
                { MicroserviceNames.CeLoadFlowService,              ServiceType.STATELESS_SERVICE   },
                { MicroserviceNames.CeMeasurementProviderService,   ServiceType.STATEFUL_SERVICE    },
                { MicroserviceNames.CeModelProviderService,         ServiceType.STATEFUL_SERVICE    },
                { MicroserviceNames.CeTopologyBuilderService,       ServiceType.STATELESS_SERVICE   },
                { MicroserviceNames.CeTopologyProviderService,      ServiceType.STATEFUL_SERVICE    },
                //TODO: OMS
                { MicroserviceNames.OmsModelProviderService,        ServiceType.STATEFUL_SERVICE},
                { MicroserviceNames.OmsOutageLifecycleService,      ServiceType.STATELESS_SERVICE},
                { MicroserviceNames.OmsCallTrackingService,         ServiceType.STATELESS_SERVICE},
                { MicroserviceNames.OmsHistoryDBManagerService,     ServiceType.STATEFUL_SERVICE},
                { MicroserviceNames.OmsOutageSimulatorService,      ServiceType.STANDALONE_SERVICE},

                //TODO: WEB_ADAPTER
                { MicroserviceNames.WebAdapterService,              ServiceType.STATELESS_SERVICE},
                //TEST
                //{ MicroserviceNames.TestService,                    ServiceType.STATELESS_SERVICE   },
            };

            //TODO: moguce ucitavanje iz konfiguracije
            serviceNameToServiceUri = new Dictionary<string, Uri>
            {
                //NMS
                { MicroserviceNames.NmsGdaService,                  new Uri("fabric:/OMS.Cloud/NMS.GdaService")                 },
                
                //SCADA
                { MicroserviceNames.ScadaModelProviderService,      new Uri("fabric:/OMS.Cloud/SCADA.ModelProviderService")     },
                { MicroserviceNames.ScadaFunctionExecutorService,   new Uri("fabric:/OMS.Cloud/SCADA.FunctionExecutorService")  },
                { MicroserviceNames.ScadaCommandingService,         new Uri("fabric:/OMS.Cloud/SCADA.CommandingService")        },
                { MicroserviceNames.ScadaAcquisitionService,        new Uri("fabric:/OMS.Cloud/SCADA.AcquisitionService")       },
                
                //PUB_SUB
                { MicroserviceNames.PubSubService,                  new Uri("fabric:/OMS.Cloud/PubSubService")                  },
                
                //TMS
                { MicroserviceNames.TransactionManagerService,      new Uri("fabric:/OMS.Cloud/TMS.TransactionManagerService")  },

                //TODO: CE
                { MicroserviceNames.CeLoadFlowService,              new Uri("fabric:/OMS.Cloud/CE.LoadFlowService")                },
                { MicroserviceNames.CeMeasurementProviderService,   new Uri("fabric:/OMS.Cloud/CE.MeasurementProviderService")     },
                { MicroserviceNames.CeModelProviderService,         new Uri("fabric:/OMS.Cloud/CE.ModelProviderService")           },
                { MicroserviceNames.CeTopologyBuilderService,       new Uri("fabric:/OMS.Cloud/CE.TopologyBuilderService")         },
                { MicroserviceNames.CeTopologyProviderService,      new Uri("fabric:/OMS.Cloud/CE.TopologyProviderService")        },

                //TODO: OMS
                { MicroserviceNames.OmsModelProviderService,         new Uri("fabric:/OMS.Cloud/OMS.ModelProviderService")      },
                { MicroserviceNames.OmsCallTrackingService,          new Uri("fabric:/OMS.Cloud/OMS.CallTrackingService")       },
                { MicroserviceNames.OmsHistoryDBManagerService,      new Uri("fabric:/OMS.Cloud/OMS.HistoryDBManagerService")   },
                { MicroserviceNames.OmsOutageLifecycleService,       new Uri("fabric:/OMS.Cloud/OMS.OutageLifecycleService")    },
                { MicroserviceNames.OmsOutageSimulatorService,       new Uri("net.tcp://localhost:10009/OutageSimulator/")      },
                //TODO: WEB_ADAPTER
                { MicroserviceNames.WebAdapterService,               new Uri("fabric:/Cloud.Web/WebAdapterService")},

                //{ MicroserviceNames.TestService,                    new Uri("fabric:/OMS.Cloud/TestService")                    },
            };
        }
    }
}
