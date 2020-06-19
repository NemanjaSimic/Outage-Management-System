using FTN.Services.NetworkModelService.TestClientUI;
using Outage.Common;
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
using TelventDMS.Services.NetworkModelService.TestClient.TestsUI;

namespace NMSTestClientUI.UserControls
{
    /// <summary>
    /// Interaction logic for GetExtentValues.xaml
    /// </summary>
    public partial class GetExtentValues : UserControl
    {
        private TestGda tgda;
        private ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
        private Dictionary<ModelCode, string> propertiesDesc = new Dictionary<ModelCode, string>();

        public ObservableCollection<ClassTypeViewModel> ClassTypes { get; private set; }

        public ClassTypeViewModel SelectedType { get; set; }

        public GetExtentValues()
        {
            InitializeComponent();
            DataContext = this;

            ClassTypes = new ObservableCollection<ClassTypeViewModel>();

            try
            {
                tgda = new TestGda();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "GetExtentValues", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            foreach (DMSType dmsType in Enum.GetValues(typeof(DMSType)))
            {
                if (dmsType == DMSType.MASK_TYPE)
                {
                    continue;
                }

                ModelCode dmsTypesModelCode = modelResourcesDesc.GetModelCodeFromType(dmsType);
                ClassTypes.Add(new ClassTypeViewModel() { ClassType = dmsTypesModelCode });
            }

            SelectedType = null;
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(SelectedType == null)
            {
                return;
            }

            Properties.Children.Clear();

            Label label = new Label()
            {
                FontWeight = FontWeights.UltraBold,
                Content = "Properties (for selected class)",
            };
            Properties.Children.Add(label);

            List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(ModelCodeHelper.GetTypeFromModelCode(SelectedType.ClassType));

            propertiesDesc.Clear();

            foreach (ModelCode property in properties)
            {
                propertiesDesc.Add(property, property.ToString());

                CheckBox checkBox = new CheckBox()
                {
                    Content = property.ToString(),
                };
                checkBox.Unchecked += CheckBox_Unchecked;
                Properties.Children.Add(checkBox);
            }
            CheckAllBtn.IsEnabled = true;

        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if((sender as CheckBox).IsChecked == false)
            {
                CheckAllBtn.IsEnabled = true;
            }
        }

        private void CheckAllBtn_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;
            UncheckAllBtn.IsEnabled = true;

            foreach (var child in Properties.Children)
            {
                CheckBox checkBox;
                if (child is CheckBox)
                {
                    checkBox = child as CheckBox;
                    checkBox.IsChecked = true;
                }
            }
        }

        private void UncheckAllBtn_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;
            CheckAllBtn.IsEnabled = true;

            foreach (var child in Properties.Children)
            {
                CheckBox checkBox;
                if (child is CheckBox)
                {
                    checkBox = child as CheckBox;
                    checkBox.IsChecked = false;
                }
            }
        }

        private void ButtonGetExtentValues_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedType == null)
            {
                return;
            }

            List<ModelCode> selectedProperties = new List<ModelCode>();

            foreach (var child in Properties.Children)
            {
                CheckBox checkBox;
                if (child is CheckBox)
                {
                    checkBox = child as CheckBox;
                    if (checkBox.IsChecked.Value)
                    {
                        foreach (KeyValuePair<ModelCode, string> keyValuePair in propertiesDesc)
                        {
                            if (keyValuePair.Value.Equals(checkBox.Content))
                            {
                                selectedProperties.Add(keyValuePair.Key);
                            }
                        }
                    }
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("Returned entities" + Environment.NewLine + Environment.NewLine);

            try
            {
                tgda.GetExtentValues(SelectedType.ClassType, selectedProperties, sb);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "GetValues", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            ExtentValues.Document.Blocks.Clear();
            ExtentValues.AppendText(sb.ToString());
        }

        private void ButtonRefreshTypes_Click(object sender, RoutedEventArgs e)
        {
            ClassTypes.Clear();

            foreach (DMSType dmsType in Enum.GetValues(typeof(DMSType)))
            {
                if (dmsType == DMSType.MASK_TYPE)
                {
                    continue;
                }

                ModelCode dmsTypesModelCode = modelResourcesDesc.GetModelCodeFromType(dmsType);
                ClassTypes.Add(new ClassTypeViewModel() { ClassType = dmsTypesModelCode });
            }

            SelectedType = null;
        }
    }
}
