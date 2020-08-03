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
                { MicroserviceNames.LoadFlowService,                ServiceType.STATELESS_SERVICE},
                { MicroserviceNames.MeasurementProviderService,     ServiceType.STATEFUL_SERVICE},
                { MicroserviceNames.ModelProviderService,           ServiceType.STATEFUL_SERVICE},
                { MicroserviceNames.TopologyBuilderService,         ServiceType.STATELESS_SERVICE},
                { MicroserviceNames.TopologyProviderService,        ServiceType.STATEFUL_SERVICE},
                //TODO: OMS

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
                { MicroserviceNames.LoadFlowService,                 new Uri("fabric:/OMS.Cloud/LoadFlowService")},
                { MicroserviceNames.MeasurementProviderService,      new Uri("fabric:/OMS.Cloud/MeasurementProviderService")},
                { MicroserviceNames.ModelProviderService,            new Uri("fabric:/OMS.Cloud/ModelProviderService")},
                { MicroserviceNames.TopologyBuilderService,          new Uri("fabric:/OMS.Cloud/TopologyBuilderService")},
                { MicroserviceNames.TopologyProviderService,         new Uri("fabric:/OMS.Cloud/TopologyProviderService")},

                //TODO: OMS

                //TODO: WEB_ADAPTER
                { MicroserviceNames.WebAdapterService,               new Uri("fabric:/Cloud.Web/WebAdapterService")},

                //{ MicroserviceNames.TestService,                    new Uri("fabric:/OMS.Cloud/TestService")                    },
            };
        }
    }
}
