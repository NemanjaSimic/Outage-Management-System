using Common.OmsContracts.DataContracts.OutageSimulator;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.NmsContracts;
using OMS.Common.NmsContracts.GDA;
using OMS.Common.WcfClient.NMS;
using OMS.Common.WcfClient.OMS;
using OMS.OutageSimulator.UI.BindingModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace OMS.OutageSimulator.UI.UserControls
{
    /// <summary>
    /// Interaction logic for GenerateOutage.xaml
    /// </summary>
    public partial class GenerateOutage : UserControl
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

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
            DMSType.SYNCHRONOUSMACHINE,
        };

        private readonly MainWindow parentWindow;
        private readonly ModelResourcesDesc modelResourcesDesc;

        private readonly HashSet<long> outageElementGids;
        private readonly HashSet<long> optimumIsolationPointsGids;
        private readonly HashSet<long> defaultIsolationPointsGids;

        private ICloudLogger logger;

        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        #region Bindings
        private GlobalIDBindingModel selectedGID;
        public GlobalIDBindingModel SelectedGID
        {
            get { return selectedGID; }

            set
            {
                selectedGID = value;

                SetGenerateOutageIsEnabled();
                SetSelectOutageElementIsEnabled();
                SetDeselectOutageElementIsEnabled();
                SetAddOptimumIsolationPointIsEnabled();
                SetAddDefaultIsolationPointIsEnabled();

                //OnPropertyChanged("IsSelectOutageElementEnabled");
                //OnPropertyChanged("IsDeSelectOutageElementEnabled");
                //OnPropertyChanged("IsAddOptimumIsolationPointEnabled");
                //OnPropertyChanged("IsAddDefaultIsolationPointEnabled");
            }
        }

        private GlobalIDBindingModel selectedOutageElement;
        public GlobalIDBindingModel SelectedOutageElement
        {
            get { return selectedOutageElement; }

            set
            {
                selectedOutageElement = value;
                SetGenerateOutageIsEnabled();
                SetDeselectOutageElementIsEnabled();
                //OnPropertyChanged("IsDeSelectOutageElementEnabled");
            }
        }

        private GlobalIDBindingModel selectedOptimumIsolationPoint;
        public GlobalIDBindingModel SelectedOptimumIsolationPoint
        {
            get { return selectedOptimumIsolationPoint; }

            set
            {
                selectedOptimumIsolationPoint = value;
                SetGenerateOutageIsEnabled();
                SetRemoveOptimumIsolationPointIsEnabled();
                //OnPropertyChanged("IsRemoveOptimumIsolationPointEnabled");
            }
        }

        private GlobalIDBindingModel selectedDefaultIsolationPoint;
        public GlobalIDBindingModel SelectedDefaultIsolationPoint
        {
            get { return selectedDefaultIsolationPoint; }

            set
            {
                selectedDefaultIsolationPoint = value;
                SetGenerateOutageIsEnabled();
                SetRemoveDefaultIsolationPointIsEnabled();
                //OnPropertyChanged("IsRemoveDefaultIsolationPointEnabled");
            }
        }

        public ObservableCollection<GlobalIDBindingModel> GlobalIdentifiers { get; set; }
        public ObservableCollection<GlobalIDBindingModel> OutageElement { get; set; }
        public ObservableCollection<GlobalIDBindingModel> OptimumIsolationPoints { get; set; }
        public ObservableCollection<GlobalIDBindingModel> DefaultIsolationPoints { get; set; }

        #region IsEnabled
        public bool IsSelectOutageElementEnabled
        {
            get
            {
                return SelectedGID != null &&
                       !outageElementGids.Contains(SelectedGID.GID) &&
                       IsOutageElementType(SelectedGID.GID);
            }
        }

        public bool IsDeSelectOutageElementEnabled
        {
            get
            {
                return OutageElement.Count > 0 &&
                       outageElementGids.Count > 0;
            }
        }

        public bool IsAddOptimumIsolationPointEnabled
        {
            get
            {
                return SelectedGID != null &&
                       !optimumIsolationPointsGids.Contains(SelectedGID.GID) &&
                       IsIsolationPointType(SelectedGID.GID);
            }
        }

        public bool IsRemoveOptimumIsolationPointEnabled
        {
            get
            {
                return SelectedOptimumIsolationPoint != null &&
                       optimumIsolationPointsGids.Contains(SelectedOptimumIsolationPoint.GID);
            }
        }

        public bool IsAddDefaultIsolationPointEnabled
        {
            get
            {
                return SelectedGID != null &&
                       !defaultIsolationPointsGids.Contains(SelectedGID.GID) &&
                       IsIsolationPointType(SelectedGID.GID);
            }
        }

        public bool IsRemoveDefaultIsolationPointEnabled
        {
            get
            {
                return SelectedDefaultIsolationPoint != null &&
                       defaultIsolationPointsGids.Contains(SelectedDefaultIsolationPoint.GID);
            }
        }

        public bool IsGenerateOutageEnabled
        {
            get
            {
                return OutageElement.Count == 1 &&
                       OptimumIsolationPoints.Count > 0 &&
                       DefaultIsolationPoints.Count > 0;
            }
        }

        private void SetSelectOutageElementIsEnabled()
        {
            SelectButton.IsEnabled = IsSelectOutageElementEnabled;
        }

        private void SetDeselectOutageElementIsEnabled()
        {
            DeselectButton.IsEnabled = IsDeSelectOutageElementEnabled;
        }

        private void SetAddOptimumIsolationPointIsEnabled()
        {
            AddOptimumButton.IsEnabled = IsAddOptimumIsolationPointEnabled;
        }

        private void SetRemoveOptimumIsolationPointIsEnabled()
        {
            RemoveOptimumButton.IsEnabled = IsRemoveOptimumIsolationPointEnabled;
        }

        private void SetAddDefaultIsolationPointIsEnabled()
        {
            AddDefaultButton.IsEnabled = IsAddDefaultIsolationPointEnabled;
        }

        private void SetRemoveDefaultIsolationPointIsEnabled()
        {
            RemoveDefaultButton.IsEnabled = IsRemoveDefaultIsolationPointEnabled;
        }

        private void SetGenerateOutageIsEnabled()
        {
            GenerateButton.IsEnabled = IsGenerateOutageEnabled;
        }
        #endregion IsEnabled
        #endregion Bindings

        public GenerateOutage(MainWindow parentWindow)
        {
            InitializeComponent();
            DataContext = this;

            this.parentWindow = parentWindow;
            this.modelResourcesDesc = new ModelResourcesDesc();

            GlobalIdentifiers = new ObservableCollection<GlobalIDBindingModel>();
            OutageElement = new ObservableCollection<GlobalIDBindingModel>();
            OptimumIsolationPoints = new ObservableCollection<GlobalIDBindingModel>();
            DefaultIsolationPoints = new ObservableCollection<GlobalIDBindingModel>();

            this.outageElementGids = new HashSet<long>();
            this.optimumIsolationPointsGids = new HashSet<long>();
            this.defaultIsolationPointsGids = new HashSet<long>();

            Dispatcher.BeginInvoke((Action)(() =>
            {
                InitializeGlobalIdentifiers();
            }));
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

            //modo: potencijalno vise outage elemenata?
            OutageElement.Clear();

            OutageElement.Add(SelectedGID);
            outageElementGids.Add(SelectedGID.GID);

            SetGenerateOutageIsEnabled();
            SetSelectOutageElementIsEnabled();
            SetDeselectOutageElementIsEnabled();
            //OnPropertyChanged("IsGenerateOutageEnabled");
            //OnPropertyChanged("IsSelectOutageElementEnabled");
            //OnPropertyChanged("IsDeSelectOutageElementEnabled");
        }

        private void DeSelectOutageElementButton_Click(object sender, RoutedEventArgs e)
        {
            OutageElement.Clear();
            outageElementGids.Clear();

            SetGenerateOutageIsEnabled();
            SetDeselectOutageElementIsEnabled();
            //OnPropertyChanged("IsGenerateOutageEnabled");
            //OnPropertyChanged("IsDeSelectOutageElementEnabled");
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

            SetGenerateOutageIsEnabled();
            SetAddOptimumIsolationPointIsEnabled();
            //OnPropertyChanged("IsGenerateOutageEnabled");
            //OnPropertyChanged("IsAddOptimumIsolationPointEnabled");
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

            SetGenerateOutageIsEnabled();
            SetRemoveOptimumIsolationPointIsEnabled();
            //OnPropertyChanged("IsGenerateOutageEnabled");
            //OnPropertyChanged("IsRemoveOptimumIsolationPointEnabled");
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

            SetGenerateOutageIsEnabled();
            SetAddDefaultIsolationPointIsEnabled();
            //OnPropertyChanged("IsGenerateOutageEnabled");
            //OnPropertyChanged("IsAddDefaultIsolationPointEnabled");
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

            SetGenerateOutageIsEnabled();
            SetRemoveDefaultIsolationPointIsEnabled();
            //OnPropertyChanged("IsGenerateOutageEnabled");
            //OnPropertyChanged("IsRemoveDefaultIsolationPointEnabled");
        }

        private void GenerateOutageButton_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(async () =>
            {
                await GenerateOutageLogic();
            }));
        }

        private async Task GenerateOutageLogic()
        {
            if (!(OutageElement.Count == 1 && OptimumIsolationPoints.Count > 0 && DefaultIsolationPoints.Count > 0))
            {
                string message = $"Rules: OutageElement.Count == 1 && OptimumIsolationPoints.Count > 0 && DefaultIsolationPoints.Count > 0";
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var outage = new SimulatedOutage();
            outage.OutageElementGid = OutageElement.First().GID;
            outage.OptimumIsolationPointGids.AddRange(OptimumIsolationPoints.Select(point => point.GID));
            outage.DefaultIsolationPointGids.AddRange(DefaultIsolationPoints.Select(point => point.GID));

            for (int i = 0; i < DefaultIsolationPoints.Count; i++)
            {
                if (!outage.DefaultToOptimumIsolationPointMap.ContainsKey(DefaultIsolationPoints[i].GID) && i < OptimumIsolationPoints.Count)
                {
                    outage.DefaultToOptimumIsolationPointMap.Add(DefaultIsolationPoints[i].GID, OptimumIsolationPoints[i].GID);
                }
            }

            var outageSimulatorUIClient = OutageSimulatorUIClient.CreateClient();
            await outageSimulatorUIClient.GenerateOutage(outage);

            await parentWindow.ChangeTab(TabType.OVERVIEW);

            OutageElement.Clear();
            outageElementGids.Clear();
            OptimumIsolationPoints.Clear();
            optimumIsolationPointsGids.Clear();
            DefaultIsolationPoints.Clear();
            defaultIsolationPointsGids.Clear();

            SetGenerateOutageIsEnabled();
            SetSelectOutageElementIsEnabled();
            SetDeselectOutageElementIsEnabled();
            SetAddOptimumIsolationPointIsEnabled();
            SetRemoveOptimumIsolationPointIsEnabled();
            SetAddDefaultIsolationPointIsEnabled();
            SetRemoveDefaultIsolationPointIsEnabled();
            //OnPropertyChanged("IsGenerateOutageEnabled");
            //OnPropertyChanged("IsSelectOutageElementEnabled");
            //OnPropertyChanged("IsDeSelectOutageElementEnabled");
            //OnPropertyChanged("IsAddOptimumIsolationPointEnabled");
            //OnPropertyChanged("IsRemoveOptimumIsolationPointEnabled");
            //OnPropertyChanged("IsAddDefaultIsolationPointEnabled");
            //OnPropertyChanged("IsRemoveDefaultIsolationPointEnabled");
        }

        private void ButtonRefreshGids_Click(object sender, RoutedEventArgs e)
        {
            GlobalIdentifiers.Clear();
            InitializeGlobalIdentifiers();
        }
        #endregion OnButtonClick

        #region Private Methods
        private Task InitializeGlobalIdentifiers()
        {
            return Task.Run(async () =>
            {
                var gdaClient = NetworkModelGdaClient.CreateClient();

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
                    int numberOfResources = 10000; //MODO: connfigurabilno

                    try
                    {
                        iteratorId = await gdaClient.GetExtentValues(dmsTypesModelCode, propIds);
                        resourcesLeft = await gdaClient.IteratorResourcesLeft(iteratorId);

                        while (resourcesLeft > 0)
                        {
                            List<ResourceDescription> gdaResult = await gdaClient.IteratorNext(numberOfResources, iteratorId);

                            foreach (ResourceDescription rd in gdaResult)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    GlobalIdentifiers.Add(new GlobalIDBindingModel(rd.Id));
                                });
                            }

                            resourcesLeft = await gdaClient.IteratorResourcesLeft(iteratorId);
                        }

                        await gdaClient.IteratorClose(iteratorId);
                    }
                    catch (Exception e)
                    {
                        string message = string.Format("Getting extent values method failed for {0}.\n\t{1}", dmsTypesModelCode, e.Message);
                        Logger.LogError(message);
                    }
                }
            });
        }

        private bool IsIsolationPointType(long gid)
        {
            DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(gid);
            switch (type)
            {
                case DMSType.BREAKER:
                case DMSType.DISCONNECTOR:
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
                    return true;
                default:
                    return false;
            }
        }
        #endregion Private Methods
    }
}
