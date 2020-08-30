using OMS.Common.WcfClient.OMS;
using OMS.OutageSimulator.UI.UserControls;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OMS.OutageSimulator.UI
{
    internal enum TabType : int
    {
        OVERVIEW = 0,
        GENERATE_OUTAGE = 1,
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private readonly Overview overview;
        //private readonly GenerateOutage generateOutage;

        private TabItem overviewTab;
        private TabItem generateOutageTab;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            
            this.ResizeMode = ResizeMode.NoResize;
            //this.overview = new Overview(this);
            //this.generateOutage = new GenerateOutage(this);

            InitializeTabControl();
        }

        internal async Task ChangeTab(TabType tabType)
        {
            if(tabType == TabType.OVERVIEW)
            {
                await Dispatcher.BeginInvoke((Action)(async () =>
                {
                    await OverviewSelection();
                    TabControl.SelectedIndex = (int)TabType.OVERVIEW;
                }));
            }
            else if(tabType == TabType.GENERATE_OUTAGE)
            {
                await Dispatcher.BeginInvoke((Action)(() => TabControl.SelectedIndex = (int)TabType.GENERATE_OUTAGE));
            }
        }

        private void InitializeTabControl()
        {
            this.overviewTab = new TabItem()
            {
                Header = "Overview",
                Content = new Overview(this),
            };
            this.overviewTab.MouseLeftButtonUp += OverviewTabControl_MouseLeftButtonUp;

            this.generateOutageTab = new TabItem()
            {
                Header = "Generate Outage",
                Content = new GenerateOutage(this),
            };

            TabControl.Items.Add(this.overviewTab);
            TabControl.Items.Add(this.generateOutageTab);
        }

        private void OverviewTabControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (overviewTab.IsSelected)
            {
                Dispatcher.BeginInvoke((Action)(async () =>
                {
                    await OverviewSelection();
                }));
            }
        }

        private async Task OverviewSelection()
        {
            var outageSimulatorUIClient = OutageSimulatorUIClient.CreateClient();
            var simulatedOutages = await outageSimulatorUIClient.GetAllSimulatedOutages();

            ((Overview)this.overviewTab.Content).SetOutages(simulatedOutages);
        }
    }
}
