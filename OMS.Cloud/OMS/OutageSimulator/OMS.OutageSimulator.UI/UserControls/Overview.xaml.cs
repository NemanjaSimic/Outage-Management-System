using Common.OmsContracts.DataContracts.OutageSimulator;
using OMS.Common.Cloud.Logger;
using OMS.Common.WcfClient.OMS;
using OMS.OutageSimulator.UI.BindingModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace OMS.OutageSimulator.UI.UserControls
{
    /// <summary>
    /// Interaction logic for Overview.xaml
    /// </summary>
    public partial class Overview : UserControl
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        private readonly MainWindow parentWindow;

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        private Dictionary<long, ActiveOutageBindingModel> activeOutagesMap;


        #region Bindings
        public List<GlobalIDBindingModel> OptimumIsolationPoints
        {
            get { return SelectedOutage?.OptimumIsolationPoints; }
        }

        public List<GlobalIDBindingModel> DefaultIsolationPoints
        {
            get { return SelectedOutage?.DefaultIsolationPoints; }
        }

        public ActiveOutageBindingModel SelectedOutage { get; set; }

        public ObservableCollection<ActiveOutageBindingModel> ActiveOutages { get; set; }

        public Visibility IsSelectedOutageGridVisible
        {
            get { return SelectedOutage != null ? Visibility.Visible : Visibility.Hidden; }
        }
        #endregion

        public Overview(MainWindow parentWindow)
        {
            InitializeComponent();
            DataContext = this;

            this.parentWindow = parentWindow;

            activeOutagesMap = new Dictionary<long, ActiveOutageBindingModel>();
            ActiveOutages = new ObservableCollection<ActiveOutageBindingModel>();
        }

        public void SetOutages(IEnumerable<SimulatedOutage> simulatedOutages)
        {
            activeOutagesMap.Clear();
            ActiveOutages.Clear();

            foreach (var simulatedOutage in simulatedOutages)
            {
                var outage = new ActiveOutageBindingModel()
                {
                    OutageElement = new GlobalIDBindingModel(simulatedOutage.OutageElementGid),
                    OptimumIsolationPoints = new List<GlobalIDBindingModel>(simulatedOutage.OptimumIsolationPointGids.Select(gid => new GlobalIDBindingModel(gid))),
                    DefaultIsolationPoints = new List<GlobalIDBindingModel>(simulatedOutage.DefaultIsolationPointGids.Select(gid => new GlobalIDBindingModel(gid))),
                    DefaultToOptimumIsolationPointMap = new Dictionary<long, long>(simulatedOutage.DefaultToOptimumIsolationPointMap),
                };

                ActiveOutages.Add(outage);
                activeOutagesMap.Add(simulatedOutage.OutageElementGid, outage);
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnPropertyChanged("ActiveOutages");
            OnPropertyChanged("SelectedOutage");
            OnPropertyChanged("IsSelectedOutageGridVisible");
            OnPropertyChanged("OutageElement");
            OnPropertyChanged("OptimumIsolationPoints");
            OnPropertyChanged("DefaultIsolationPoints");
        }

        private void EndOutageButton_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(async () =>
            {
                await EndOutage();
            }));
        }

        private async Task EndOutage()
        {
            var outageElementId = SelectedOutage.OutageElement.GID;

            var outageSimulatorUIClient = OutageSimulatorUIClient.CreateClient();
            if (await outageSimulatorUIClient.EndOutage(outageElementId))
            {
                if (!activeOutagesMap.ContainsKey(outageElementId))
                {
                    return;
                }

                ActiveOutageBindingModel outage = activeOutagesMap[outageElementId];
                ActiveOutages.Remove(outage);
                activeOutagesMap.Remove(outageElementId);

                OnPropertyChanged("ActiveOutages");
                OnPropertyChanged("SelectedOutage");
                OnPropertyChanged("IsSelectedOutageGridVisible");
                OnPropertyChanged("OutageElement");
                OnPropertyChanged("OptimumIsolationPoints");
                OnPropertyChanged("DefaultIsolationPoints");
            }
        }
    }
}
