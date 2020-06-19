using OMS.OutageSimulator.BindingModels;
using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceContracts.GDA;
using Outage.Common.ServiceProxies;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace OMS.OutageSimulator.UserControls
{
    /// <summary>
    /// Interaction logic for GenerateOutage.xaml
    /// </summary>
    public partial class GenerateOutage : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
        
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
            DMSType.FUSE,
            DMSType.POWERTRANSFORMER,
            DMSType.TERMINAL,
            DMSType.ENERGYCONSUMER,
            DMSType.ENERGYSOURCE,
            DMSType.TRANSFORMERWINDING,
        };

        private ProxyFactory proxyFactory;
        private ModelResourcesDesc modelResourcesDesc;
        private Overview overviewUserControl;

        private HashSet<long> outageElementGids;
        private HashSet<long> optimumIsolationPointsGids;
        private HashSet<long> defaultIsolationPointsGids;

        #region Bindings
        private GlobalIDBindingModel selectedGID;
        public GlobalIDBindingModel SelectedGID 
        {
            get { return selectedGID; }
            
            set
            {
                selectedGID = value;
                OnPropertyChanged("IsSelectOutageElementEnabled");
                OnPropertyChanged("IsDeSelectOutageElementEnabled");
                OnPropertyChanged("IsAddOptimumIsolationPointEnabled");
                OnPropertyChanged("IsAddDefaultIsolationPointEnabled");
            }
        }

        private GlobalIDBindingModel selectedOutageElement;
        public GlobalIDBindingModel SelectedOutageElement
        {
            get { return selectedOutageElement; }

            set
            {
                selectedOutageElement = value;
                OnPropertyChanged("IsDeSelectOutageElementEnabled");
            }
        }

        private GlobalIDBindingModel selectedOptimumIsolationPoint;
        public GlobalIDBindingModel SelectedOptimumIsolationPoint
        {
            get { return selectedOptimumIsolationPoint; }

            set
            {
                selectedOptimumIsolationPoint = value;
                OnPropertyChanged("IsRemoveOptimumIsolationPointEnabled");
            }
        }

        private GlobalIDBindingModel selectedDefaultIsolationPoint;
        public GlobalIDBindingModel SelectedDefaultIsolationPoint
        {
            get { return selectedDefaultIsolationPoint; }

            set
            {
                selectedDefaultIsolationPoint = value;
                OnPropertyChanged("IsRemoveDefaultIsolationPointEnabled");
            }
        }

        public ObservableCollection<GlobalIDBindingModel> GlobalIdentifiers { get; set; }
        public ObservableCollection<GlobalIDBindingModel> OutageElement { get; set; }
        public ObservableCollection<GlobalIDBindingModel> OptimumIsolationPoints { get; set; }
        public ObservableCollection<GlobalIDBindingModel> DefaultIsolationPoints { get; set; }

        #region IsEnabled
        //OnPropertyChanged("IsSelectOutageElementEnabled")
        public bool IsSelectOutageElementEnabled
        {
            get
            {
                return SelectedGID != null && 
                       !outageElementGids.Contains(SelectedGID.GID) && 
                       IsOutageElementType(SelectedGID.GID);
            }
        }

        //OnPropertyChanged("IsDeSelectOutageElementEnabled")
        public bool IsDeSelectOutageElementEnabled
        {
            get
            {
                return OutageElement.Count > 0 && 
                       outageElementGids.Count > 0;
            }
        }

        //OnPropertyChanged("IsAddOptimumIsolationPointEnabled")
        public bool IsAddOptimumIsolationPointEnabled
        {
            get
            {
                return SelectedGID != null && 
                       !optimumIsolationPointsGids.Contains(SelectedGID.GID) && 
                       IsIsolationPointType(SelectedGID.GID);
            }
        }

        //OnPropertyChanged("IsRemoveOptimumIsolationPointEnabled")
        public bool IsRemoveOptimumIsolationPointEnabled
        {
            get
            {
                return SelectedOptimumIsolationPoint != null && 
                       optimumIsolationPointsGids.Contains(SelectedOptimumIsolationPoint.GID);
            }
        }

        //OnPropertyChanged("IsAddDefaultIsolationPointEnabled")
        public bool IsAddDefaultIsolationPointEnabled
        {
            get
            {
                return SelectedGID != null && 
                       !defaultIsolationPointsGids.Contains(SelectedGID.GID) && 
                       IsIsolationPointType(SelectedGID.GID);
            }
        }

        //OnPropertyChanged("IsRemoveDefaultIsolationPointEnabled")
        public bool IsRemoveDefaultIsolationPointEnabled
        {
            get
            {
                return SelectedDefaultIsolationPoint != null && 
                       defaultIsolationPointsGids.Contains(SelectedDefaultIsolationPoint.GID);
            }
        }

        //OnPropertyChanged("IsGenerateOutageEnabled")
        public bool IsGenerateOutageEnabled
        {
            get
            {
                return OutageElement.Count == 1 &&
                       OptimumIsolationPoints.Count > 0 &&
                       DefaultIsolationPoints.Count > 0;
            }
        }
        #endregion
        #endregion

        public GenerateOutage(Overview overviewUserControl)
        {
            InitializeComponent();
            DataContext = this;

            this.proxyFactory = new ProxyFactory();
            this.modelResourcesDesc = new ModelResourcesDesc();
            this.overviewUserControl = overviewUserControl;

            GlobalIdentifiers = new ObservableCollection<GlobalIDBindingModel>();
            OutageElement = new ObservableCollection<GlobalIDBindingModel>();
            OptimumIsolationPoints = new ObservableCollection<GlobalIDBindingModel>();
            DefaultIsolationPoints = new ObservableCollection<GlobalIDBindingModel>();

            this.outageElementGids = new HashSet<long>();
            this.optimumIsolationPointsGids = new HashSet<long>();
            this.defaultIsolationPointsGids = new HashSet<long>();

            InitializeGlobalIdentifiers();
        }

        private async void InitializeGlobalIdentifiers()
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

                    int iteratorId;
                    int resourcesLeft;
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
                                Dispatcher.Invoke(() =>
                                {
                                    GlobalIdentifiers.Add(new GlobalIDBindingModel()
                                    {
                                        GID = rd.Id,
                                        Type = dmsTypesModelCode.ToString(),
                                    });
                                });
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

        #region OnButtonClick
        private void SelectOutageElementButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(SelectedGID != null && !outageElementGids.Contains(SelectedGID.GID) && IsOutageElementType(SelectedGID.GID)))
            {
                string message = $"Rules: SelectedGID != null && !outageElementGids.Contains(SelectedGID.GID) && IsOutageElementType(SelectedGID.GID)";
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //todo: potencijalno vise outage elemenata?
            OutageElement.Clear();

            OutageElement.Add(SelectedGID);
            outageElementGids.Add(SelectedGID.GID);

            OnPropertyChanged("IsGenerateOutageEnabled");
            OnPropertyChanged("IsSelectOutageElementEnabled");
            OnPropertyChanged("IsDeSelectOutageElementEnabled");
        }

        private void DeSelectOutageElementButton_Click(object sender, RoutedEventArgs e)
        {
            OutageElement.Clear();
            outageElementGids.Clear();

            OnPropertyChanged("IsGenerateOutageEnabled");
            OnPropertyChanged("IsDeSelectOutageElementEnabled");
        }

        private void AddOptimumIsolationPointButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(SelectedGID != null && !optimumIsolationPointsGids.Contains(SelectedGID.GID) && IsIsolationPointType(SelectedGID.GID)))
            {
                string message = $"Rules: SelectedGID != null && !optimumIsolationPointsGids.Contains(SelectedGID.GID) && IsIsolationPointType(SelectedGID.GID)";
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            OptimumIsolationPoints.Add(SelectedGID);
            optimumIsolationPointsGids.Add(SelectedGID.GID);

            OnPropertyChanged("IsGenerateOutageEnabled");
            OnPropertyChanged("IsAddOptimumIsolationPointEnabled");
        }

        private void RemoveOptimumIsolationPointButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(SelectedOptimumIsolationPoint != null && optimumIsolationPointsGids.Contains(SelectedOptimumIsolationPoint.GID)))
            {
                string message = $"Rules: SelectedOptimumIsolationPoint != null && optimumIsolationPointsGids.Contains(SelectedOptimumIsolationPoint.GID)";
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            optimumIsolationPointsGids.Remove(SelectedOptimumIsolationPoint.GID);
            //svesno neoptimalno izbacivanje iz liste
            OptimumIsolationPoints.Remove(SelectedOptimumIsolationPoint);

            OnPropertyChanged("IsGenerateOutageEnabled");
            OnPropertyChanged("IsRemoveOptimumIsolationPointEnabled");
        }

        private void AddDefaultIsolationPointButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(SelectedGID != null && !defaultIsolationPointsGids.Contains(SelectedGID.GID) && IsIsolationPointType(SelectedGID.GID)))
            {
                string message = $"Rules: SelectedGID != null && !defaultIsolationPointsGids.Contains(SelectedGID.GID) && IsIsolationPointType(SelectedGID.GID)";
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DefaultIsolationPoints.Add(SelectedGID);
            defaultIsolationPointsGids.Add(SelectedGID.GID);

            OnPropertyChanged("IsGenerateOutageEnabled");
            OnPropertyChanged("IsAddDefaultIsolationPointEnabled");
        }

        private void RemoveDefaultIsolationPointButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(SelectedDefaultIsolationPoint != null && defaultIsolationPointsGids.Contains(SelectedDefaultIsolationPoint.GID)))
            {
                string message = $"Rules: SelectedGID != null && !defaultIsolationPointsGids.Contains(SelectedGID.GID) && IsIsolationPointType(SelectedGID.GID)";
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            defaultIsolationPointsGids.Remove(SelectedDefaultIsolationPoint.GID);
            //svesno neoptimalno izbacivanje iz liste DefaultIsolationPoints
            DefaultIsolationPoints.Remove(SelectedDefaultIsolationPoint);

            OnPropertyChanged("IsGenerateOutageEnabled");
            OnPropertyChanged("IsRemoveDefaultIsolationPointEnabled");
        }

        private void GenerateOutageButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(OutageElement.Count == 1 && OptimumIsolationPoints.Count > 0 && DefaultIsolationPoints.Count > 0))
            {
                string message = $"Rules: OutageElement.Count == 1 && OptimumIsolationPoints.Count > 0 && DefaultIsolationPoints.Count > 0";
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ActiveOutageBindingModel outage = new ActiveOutageBindingModel();
            outage.OutageElement = OutageElement.First();
            outage.OptimumIsolationPoints.AddRange(OptimumIsolationPoints);
            outage.DefaultIsolationPoints.AddRange(DefaultIsolationPoints);
            
            for(int i = 0; i < DefaultIsolationPoints.Count; i++)
            {
                if(!outage.DefaultToOptimumIsolationPointMap.ContainsKey(DefaultIsolationPoints[i].GID) && i < OptimumIsolationPoints.Count)
                {
                    outage.DefaultToOptimumIsolationPointMap.Add(DefaultIsolationPoints[i].GID, OptimumIsolationPoints[i].GID);
                }
            }

            overviewUserControl.GenerateOutage(outage);

            OutageElement.Clear();
            outageElementGids.Clear();
            OptimumIsolationPoints.Clear();
            optimumIsolationPointsGids.Clear();
            DefaultIsolationPoints.Clear();
            defaultIsolationPointsGids.Clear();

            OnPropertyChanged("IsGenerateOutageEnabled");
            OnPropertyChanged("IsSelectOutageElementEnabled");
            OnPropertyChanged("IsDeSelectOutageElementEnabled");
            OnPropertyChanged("IsAddOptimumIsolationPointEnabled");
            OnPropertyChanged("IsRemoveOptimumIsolationPointEnabled");
            OnPropertyChanged("IsAddDefaultIsolationPointEnabled");
            OnPropertyChanged("IsRemoveDefaultIsolationPointEnabled");
        }

        private void ButtonRefreshGids_Click(object sender, RoutedEventArgs e)
        {
            GlobalIdentifiers.Clear();
            InitializeGlobalIdentifiers();
        }
        #endregion

        private bool IsIsolationPointType(long gid)
        {
            DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(gid);
            switch (type)
            {
                case DMSType.BREAKER:
                case DMSType.DISCONNECTOR:
                //case DMSType.FUSE:
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
