using OMS.OutageSimulator.BindingModels;
using OMS.OutageSimulator.ScadaSubscriber;
using Outage.Common;
using Outage.Common.ServiceContracts.PubSub;
using Outage.Common.ServiceProxies;
using Outage.Common.ServiceProxies.PubSub;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace OMS.OutageSimulator.UserControls
{
    /// <summary>
    /// Interaction logic for ActiveOutages.xaml
    /// </summary>
    public partial class Overview : UserControl, INotifyPropertyChanged
    {
        private MainWindow parent;
        private ProxyFactory proxyFactory;
        private Dictionary<long, CancellationTokenSource> outageTokenMap;

        private ILogger logger;

        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        #region Bindings
        public List<GlobalIDBindingModel> OptimumIsolationPoints
        {
            get { return SelectedOutege?.OptimumIsolationPoints; }
        }

        public List<GlobalIDBindingModel> DefaultIsolationPoints
        {
            get { return SelectedOutege?.DefaultIsolationPoints; }
        }

        public ActiveOutageBindingModel SelectedOutege { get; set; }

        public ObservableCollection<ActiveOutageBindingModel> ActiveOutages { get; set; }
        
        public Visibility IsSelectedOutageGridVisible
        {
            get { return SelectedOutege != null ? Visibility.Visible : Visibility.Hidden; }
        }
        #endregion

        public Overview(MainWindow parentWindow)
        {
            InitializeComponent();
            DataContext = this;

            parent = parentWindow;
            proxyFactory = new ProxyFactory();
            outageTokenMap = new Dictionary<long, CancellationTokenSource>();
            ActiveOutages = new ObservableCollection<ActiveOutageBindingModel>();
        }

        public void GenerateOutage(ActiveOutageBindingModel outage)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;

            Task task = Task.Run(() =>
            {
                token.ThrowIfCancellationRequested();

                ScadaNotification scadaCallback = new ScadaNotification("OUTAGE_SIMULATOR", outage);
                SubscriberProxy scadaSubscriber = proxyFactory.CreateProxy<SubscriberProxy, ISubscriber>(scadaCallback, EndpointNames.SubscriberEndpoint);
                
                if(scadaSubscriber == null)
                {
                    string message = "GenerateOutage task => SubscriberProxy is null";
                    Logger.LogError(message);
                    return;
                }

                scadaSubscriber.Subscribe(Topic.SWITCH_STATUS);
                
                while (true)
                {
                    //TODO: OUTAGE LOGIC

                    if(token.IsCancellationRequested)
                    {
                        // Clean up here
                        scadaSubscriber.Close();
                        token.ThrowIfCancellationRequested();
                    }
                }

            }, token);

            outageTokenMap.Add(outage.OutageElement.GID, tokenSource);

            ActiveOutages.Add(outage);
            Dispatcher.BeginInvoke((Action)(() => parent.TabControl.SelectedIndex = 0));
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnPropertyChanged("IsSelectedOutageGridVisible");
            OnPropertyChanged("OutageElement");
            OnPropertyChanged("OptimumIsolationPoints");
            OnPropertyChanged("DefaultIsolationPoints");
        }

        private void EndOutageButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: END TASK HERE
            outageTokenMap[SelectedOutege.OutageElement.GID].Cancel();

            ActiveOutages.Remove(SelectedOutege);

            OnPropertyChanged("IsSelectedOutageGridVisible");
            OnPropertyChanged("OutageElement");
            OnPropertyChanged("OptimumIsolationPoints");
            OnPropertyChanged("DefaultIsolationPoints");
        }
    }
}
