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
        private readonly Overview overview;
        private readonly GenerateOutage generateOutage;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            
            this.ResizeMode = ResizeMode.NoResize;
            this.overview = new Overview(this);
            this.generateOutage = new GenerateOutage(this);

            InitializeTabControl();
        }

        internal async Task ChangeTab(TabType tabType)
        {
            if (tabType == TabType.OVERVIEW)
            {
                await Dispatcher.BeginInvoke((Action)(async () =>
                {
                    //await OverviewSelection();
                    TabControl.SelectedIndex = (int)TabType.OVERVIEW;
                }));
            }
            else if (tabType == TabType.GENERATE_OUTAGE)
            {
                await Dispatcher.BeginInvoke((Action)(() =>
                {
                    TabControl.SelectedIndex = (int)TabType.GENERATE_OUTAGE;
                }));
            }
        }

        private void InitializeTabControl()
        {
            var overviewTab = new TabItem()
            {
                Header = "Overview",
                Content = new Overview(this),
            };

            var generateOutageTab = new TabItem()
            {
                Header = "Generate Outage",
                Content = new GenerateOutage(this),
            };

            TabControl.Items.Add(overviewTab);
            TabControl.Items.Add(generateOutageTab);
        }

        private async Task OverviewSelection()
        {
            var outageSimulatorUIClient = OutageSimulatorUIClient.CreateClient();
            var simulatedOutages = await outageSimulatorUIClient.GetAllSimulatedOutages();

            this.overview.SetOutages(simulatedOutages);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int tabItem = ((sender as TabControl)).SelectedIndex;
            
            if (e.Source is TabControl)
            {
                switch (tabItem)
                {
                    case (int)TabType.OVERVIEW:
                        Dispatcher.BeginInvoke((Action)(async () =>
                        {
                            await OverviewSelection();
                        }));
                        break;

                    case (int)TabType.GENERATE_OUTAGE:
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
