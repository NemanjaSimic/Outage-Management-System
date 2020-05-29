using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Common.Cloud.WcfServiceFabricClients.NMS;
using OMS.Common.Cloud.WcfServiceFabricClients.SCADA;
using OMS.Common.NmsContracts.GDA;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.DataContracts;
using OMS.Common.ScadaContracts.ModelProvider;
using Outage.Common;
using Outage.Common.PubSub.SCADADataContract;

namespace TestService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class TestService : StatelessService
    {
        private readonly ReadCommandEnqueuerClient readCommandEnqueuerClient;
        private readonly WriteCommandEnqueuerClient writeCommandEnqueuerClient;
        private readonly ModelUpdateCommandEnqueuerClient modelUpdateCommandEnqueuerClient;
        private readonly ScadaModelReadAccessClient scadaModelReadAccessClient;
        private readonly ScadaModelUpdateAccessClient scadaModelUpdateAccessClient;
        private readonly ScadaIntegrityUpdateClient scadaIntegrityUpdateClient;
        private readonly ScadaCommandingClient scadaCommandingClient;
        private readonly NetworkModelGdaClient networkModelGdaClient;

        public TestService(StatelessServiceContext context)
            : base(context)
        {
            this.readCommandEnqueuerClient = ReadCommandEnqueuerClient.CreateClient();
            this.writeCommandEnqueuerClient = WriteCommandEnqueuerClient.CreateClient();
            this.modelUpdateCommandEnqueuerClient = ModelUpdateCommandEnqueuerClient.CreateClient();
           
            this.scadaModelReadAccessClient = ScadaModelReadAccessClient.CreateClient();
            this.scadaModelUpdateAccessClient = ScadaModelUpdateAccessClient.CreateClient();
            this.scadaIntegrityUpdateClient = ScadaIntegrityUpdateClient.CreateClient();

            this.scadaCommandingClient = ScadaCommandingClient.CreateClient();

            this.networkModelGdaClient = NetworkModelGdaClient.CreateClient();
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            //while(true)
            //{
            //    await TestScadaCommandingClient();
            //    await Task.Delay(2000);
            //}

            //await TestReadCommandEnqueuerClient();
            //await TestWriteCommandEnqueuerClient();
            //await TestModelUpdateCommandEnqueuerClient();

            //await TestScadaModelReadAccessClient();
            //await TestScadaModelUpdateAccessClient();
            //await TestScadaIntegrityUpdateClient();
            //await TestScadaCommandingClient();
            //await TestNetworkModelGdaClient();

            //try
            //{
            //    Type type1 = typeof(IScadaModelReadAccessContract);
            //    Type type2 = typeof(IScadaModelUpdateAccessContract);

            //    ServiceContractAttribute atr1 =  (ServiceContractAttribute)Attribute.GetCustomAttribute(type1, typeof(ServiceContractAttribute));
            //    ServiceContractAttribute atr2 = (ServiceContractAttribute)Attribute.GetCustomAttribute(type2, typeof(ServiceContractAttribute));

            //    Attribute[] atr1s = Attribute.GetCustomAttributes(type1);
            //    Attribute[] atr2s = Attribute.GetCustomAttributes(type2);

            //    OperationContractAttribute op1 = (OperationContractAttribute)Attribute.GetCustomAttribute(type1.GetMethod("GetAddressToGidMap"), typeof(OperationContractAttribute));
            //    OperationContractAttribute op2 = (OperationContractAttribute)Attribute.GetCustomAttribute(type1.GetMethod("GetAddressToPointItemMap"), typeof(OperationContractAttribute));
            //    OperationContractAttribute op3 = (OperationContractAttribute)Attribute.GetCustomAttribute(type1.GetMethod("GetCommandDescriptionCache"), typeof(OperationContractAttribute));
            //    OperationContractAttribute op4 = (OperationContractAttribute)Attribute.GetCustomAttribute(type1.GetMethod("GetGidToPointItemMap"), typeof(OperationContractAttribute));
                                                 
            //    OperationContractAttribute op5 = (OperationContractAttribute)Attribute.GetCustomAttribute(type2.GetMethod("MakeAnalogEntryToMeasurementCache"), typeof(OperationContractAttribute));
            //    OperationContractAttribute op6 = (OperationContractAttribute)Attribute.GetCustomAttribute(type2.GetMethod("MakeDiscreteEntryToMeasurementCache"), typeof(OperationContractAttribute));
            //    OperationContractAttribute op7 = (OperationContractAttribute)Attribute.GetCustomAttribute(type2.GetMethod("UpdateCommandDescription"), typeof(OperationContractAttribute));
            //}
            //catch (Exception e)
            //{
            //}
        }

        private async Task TestReadCommandEnqueuerClient()
        {
            try
            {
                await this.readCommandEnqueuerClient.EnqueueReadCommand(null);
            }
            catch (Exception e)
            {
            }
        }

        private async Task TestWriteCommandEnqueuerClient()
        {
            try
            {
                await this.writeCommandEnqueuerClient.EnqueueWriteCommand(null);
            }
            catch (Exception e)
            {
            }
        }

        private async Task TestModelUpdateCommandEnqueuerClient()
        {
            try
            {
                await this.modelUpdateCommandEnqueuerClient.EnqueueModelUpdateCommands(new List<IWriteModbusFunction>());
            }
            catch (Exception e)
            {
            }
        }

        private async Task TestScadaModelReadAccessClient()
        {
            //try
            //{
            //    await this.scadaModelReadAccessClient.GetAddressToGidMap();
            //}
            //catch (Exception e)
            //{
            //}

            //The given key was not present in the dictionary. je bug koji ne bi trebalo da se javi ni u testu
            try
            {
                await this.scadaModelReadAccessClient.GetAddressToPointItemMap();
            }
            catch (Exception e)
            {
            }

            try
            {
                await this.scadaModelReadAccessClient.GetCommandDescriptionCache();
            }
            catch (Exception e)
            {
            }

            try
            {
                await this.scadaModelReadAccessClient.GetGidToPointItemMap();
            }
            catch (Exception e)
            {
            }

            try
            {
                await this.scadaModelReadAccessClient.GetIsScadaModelImportedIndicator();
            }
            catch (Exception e)
            {
            }

            try
            {
                await this.scadaModelReadAccessClient.GetScadaConfigData();
            }
            catch (Exception e)
            {
            }
        }

        private async Task TestScadaModelUpdateAccessClient()
        {
            try
            {
                await this.scadaModelUpdateAccessClient.MakeAnalogEntryToMeasurementCache(new Dictionary<long, AnalogModbusData>(), false);
            }
            catch (Exception e)
            {
            }

            try
            {
                await this.scadaModelUpdateAccessClient.MakeDiscreteEntryToMeasurementCache(new Dictionary<long, DiscreteModbusData>(), false);
            }
            catch (Exception e)
            {
            }

            try
            {
                await this.scadaModelUpdateAccessClient.UpdateCommandDescription(0, new CommandDescription());
            }
            catch (Exception e)
            {
            }
        }

        private async Task TestScadaIntegrityUpdateClient()
        {
            try
            {
                await this.scadaIntegrityUpdateClient.GetIntegrityUpdate();
            }
            catch (Exception e)
            {
            }

            try
            {
                await this.scadaIntegrityUpdateClient.GetIntegrityUpdateForSpecificTopic(Topic.ACTIVE_OUTAGE);
            }
            catch (Exception e)
            {
            }
        }

        private async Task TestScadaCommandingClient()
        {
            try
            {
                await this.scadaCommandingClient.SendSingleAnalogCommand(0x0000000E00000001, 666, CommandOriginType.OTHER_COMMAND);
            }
            catch (Exception e)
            {
            }

            //try
            //{
            //    await this.scadaCommandingClient.SendMultipleAnalogCommand(new Dictionary<long, float>(), CommandOriginType.OTHER_COMMAND);
            //}
            //catch (Exception e)
            //{
            //}

            try
            {
                await this.scadaCommandingClient.SendSingleDiscreteCommand(0x0000000D00000001, 1, CommandOriginType.OTHER_COMMAND);
            }
            catch (Exception e)
            {
            }

            //try
            //{
            //    await this.scadaCommandingClient.SendMultipleDiscreteCommand(new Dictionary<long, ushort>(), CommandOriginType.OTHER_COMMAND);
            //}
            //catch (Exception e)
            //{
            //}
        }

        private async Task TestNetworkModelGdaClient()
        {
            //try
            //{
            //    await this.networkModelGdaClient.GetValues(0, new List<ModelCode>());
            //}
            //catch (Exception e)
            //{
            //}

            try
            {
                await this.networkModelGdaClient.GetExtentValues(ModelCode.ACLINESEGMENT, new List<ModelCode>());
            }
            catch (Exception e)
            {
            }

            try
            {
                await this.networkModelGdaClient.GetRelatedValues(0, new List<ModelCode>(), new Association());
            }
            catch (Exception e)
            {
            }
        }
    }
}
