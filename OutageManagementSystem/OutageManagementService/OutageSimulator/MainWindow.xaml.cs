using OMS.OutageSimulator.UserControls;
using System.Windows;
using System.Windows.Controls;

namespace OMS.OutageSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            this.ResizeMode = ResizeMode.NoResize;
            
            InitializeTabControl();
        }

        private void InitializeTabControl()
        {
            TabItem overview = new TabItem()
            {
                Header = "Overview",
                Content = new Overview(this),
            };
            
            TabItem generateOutage = new TabItem()
            {
                Header = "Generate Outage",
                Content = new GenerateOutage(overview.Content as Overview),
            };
            

            TabControl.Items.Add(overview);
            TabControl.Items.Add(generateOutage);
        }
    }
}
