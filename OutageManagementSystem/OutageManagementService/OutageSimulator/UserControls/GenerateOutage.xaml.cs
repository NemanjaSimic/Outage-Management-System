using OMS.OutageSimulator.BindingModels;
using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceContracts.GDA;
using Outage.Common.ServiceProxies;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OMS.OutageSimulator.UserControls
{
    /// <summary>
    /// Interaction logic for GenerateOutage.xaml
    /// </summary>
    public partial class GenerateOutage : UserControl
    {
        private ILogger logger;

        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private readonly HashSet<DMSType> ignorableTypes = new HashSet<DMSType>()
        {
            DMSType.ANALOG,
            DMSType.BASEVOLTAGE,
            DMSType.CONNECTIVITYNODE,
            DMSType.DISCRETE,
            DMSType.ENERGYSOURCE,
            DMSType.POWERTRANSFORMER,
            DMSType.TERMINAL,
            DMSType.ENERGYCONSUMER,
            DMSType.ENERGYSOURCE,
            DMSType.TRANSFORMERWINDING,
        };

        private ProxyFactory proxyFactory;
        private ModelResourcesDesc modelResourcesDesc;

        private HashSet<long> outageElementGids;
        private HashSet<long> optimumIsolationPointsGids;
        private HashSet<long> defaultIsolationPointsGids;

        #region Bindings
        public GlobalIDBindingModel SelectedGID { get; set; }
        public GlobalIDBindingModel SelectedOutageElement { get; set; }
        public GlobalIDBindingModel SelectedOptimumIsolationPoint { get; set; }
        public GlobalIDBindingModel SelectedDefaultIsolationPoint { get; set; }


        public ObservableCollection<GlobalIDBindingModel> GlobalIdentifiers { get; set; }
        public ObservableCollection<GlobalIDBindingModel> OutageElement { get; set; }
        public ObservableCollection<GlobalIDBindingModel> OptimumIsolationPoints { get; set; }
        public ObservableCollection<GlobalIDBindingModel> DefaultIsolationPoints { get; set; }
        #endregion

        public GenerateOutage()
        {
            InitializeComponent();
            DataContext = this;
            
            proxyFactory = new ProxyFactory();
            modelResourcesDesc = new ModelResourcesDesc();

            GlobalIdentifiers = new ObservableCollection<GlobalIDBindingModel>();
            OutageElement = new ObservableCollection<GlobalIDBindingModel>();
            OptimumIsolationPoints = new ObservableCollection<GlobalIDBindingModel>();
            DefaultIsolationPoints = new ObservableCollection<GlobalIDBindingModel>();

            outageElementGids = new HashSet<long>();
            optimumIsolationPointsGids = new HashSet<long>();
            defaultIsolationPointsGids = new HashSet<long>();

            InitializeGlobalIdentifiers();
        }

        private void InitializeGlobalIdentifiers()
        {
            using (NetworkModelGDAProxy gdaProxy = proxyFactory.CreateProxy<NetworkModelGDAProxy, INetworkModelGDAContract>(EndpointNames.NetworkModelGDAEndpoint))
            {
                if(gdaProxy == null)
                {
                    throw new NullReferenceException("InitializeGlobalIdentifiers => NetworkModelGDAProxy is null.");
                }

                List<ModelCode> propIds = new List<ModelCode> { ModelCode.IDOBJ_GID };

                foreach (DMSType dmsType in Enum.GetValues(typeof(DMSType)))
                {
                    if (dmsType == DMSType.MASK_TYPE || ignorableTypes.Contains(dmsType))
                    {
                        continue;
                    }

                    ModelCode dmsTypesModelCode = modelResourcesDesc.GetModelCodeFromType(dmsType);

                    int iteratorId = 0;
                    int resourcesLeft = 0;
                    int numberOfResources = 10000; //TODO: connfigurabilno

                    try
                    {
                        iteratorId = gdaProxy.GetExtentValues(dmsTypesModelCode, propIds);
                        resourcesLeft = gdaProxy.IteratorResourcesLeft(iteratorId);

                        while (resourcesLeft > 0)
                        {
                            List<ResourceDescription> gdaResult = gdaProxy.IteratorNext(numberOfResources, iteratorId);

                            foreach (ResourceDescription rd in gdaResult)
                            {
                                GlobalIDBindingModel globalIdentifier = new GlobalIDBindingModel()
                                {
                                    GID = rd.Id,
                                    Type = dmsTypesModelCode.ToString(),
                                };

                                GlobalIdentifiers.Add(globalIdentifier);
                            }

                            resourcesLeft = gdaProxy.IteratorResourcesLeft(iteratorId);
                        }

                        gdaProxy.IteratorClose(iteratorId);
                    }
                    catch (Exception e)
                    {
                        string message = string.Format("Getting extent values method failed for {0}.\n\t{1}", dmsTypesModelCode, e.Message);
                        Logger.LogError(message);
                    }
                }
            }
        }

        private void SelectOutageElementButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedGID != null && !outageElementGids.Contains(SelectedGID.GID) && IsOutageElementType(SelectedGID.GID))
            {
                //todo: potencijalno vise outage elemenata?
                OutageElement.Clear();

                OutageElement.Add(SelectedGID);
                outageElementGids.Add(SelectedGID.GID);
            }
        }

        private void DeSelectOutageElementButton_Click(object sender, RoutedEventArgs e)
        {
            OutageElement.Clear();
            outageElementGids.Clear();
        }

        private void AddOptimumIsolationPointButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedGID != null && !optimumIsolationPointsGids.Contains(SelectedGID.GID) && IsIsolationPointType(SelectedGID.GID))
            {
                OptimumIsolationPoints.Add(SelectedGID);
                optimumIsolationPointsGids.Add(SelectedGID.GID);
            }
        }

        private void RemoveOptimumIsolationPointButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedOptimumIsolationPoint != null && optimumIsolationPointsGids.Contains(SelectedOptimumIsolationPoint.GID))
            {
                optimumIsolationPointsGids.Remove(SelectedOptimumIsolationPoint.GID);
                //svesno neoptimalno izbacivanje iz liste
                OptimumIsolationPoints.Remove(SelectedOptimumIsolationPoint);
            }
        }

        private void AddDefaultIsolationPointButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedGID != null && !defaultIsolationPointsGids.Contains(SelectedGID.GID) && IsIsolationPointType(SelectedGID.GID))
            {
                DefaultIsolationPoints.Add(SelectedGID);
                defaultIsolationPointsGids.Add(SelectedGID.GID);
            }
        }

        private void RemoveDefaultIsolationPointButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedDefaultIsolationPoint != null && defaultIsolationPointsGids.Contains(SelectedDefaultIsolationPoint.GID))
            {
                defaultIsolationPointsGids.Remove(SelectedDefaultIsolationPoint.GID);
                //svesno neoptimalno izbacivanje iz liste DefaultIsolationPoints
                DefaultIsolationPoints.Remove(SelectedDefaultIsolationPoint);
            }
        }

        private void ButtonRefreshGids_Click(object sender, RoutedEventArgs e)
        {
            GlobalIdentifiers.Clear();
            InitializeGlobalIdentifiers();
        }

        private bool IsIsolationPointType(long gid)
        {
            DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(gid);
            switch (type)
            {
                case DMSType.BREAKER:
                case DMSType.DISCONNECTOR:
                case DMSType.FUSE:
                case DMSType.LOADBREAKSWITCH:
                    return true;
                default:
                    return false;
            }
        }

        private bool IsOutageElementType(long gid)
        {
            DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(gid);
            switch (type)
            {
                case DMSType.ACLINESEGMENT:
                //case DMSType.BREAKER:
                //case DMSType.DISCONNECTOR:
                //case DMSType.FUSE:
                //case DMSType.LOADBREAKSWITCH:
                    return true;
                default:
                    return false;
            }
        }
    }
}
