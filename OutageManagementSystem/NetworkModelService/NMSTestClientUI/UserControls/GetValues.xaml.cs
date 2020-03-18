using FTN.Services.NetworkModelService.TestClientUI;
using Outage.Common;
using Outage.Common.GDA;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using TelventDMS.Services.NetworkModelService.TestClient.TestsUI;

namespace NMSTestClientUI.UserControls
{
    /// <summary>
    /// Interaction logic for GetValues.xaml
    /// </summary>
    public partial class GetValues : UserControl
    {
        private TestGda tgda;
        private ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
        private Dictionary<ModelCode, string> propertiesDesc = new Dictionary<ModelCode, string>();

        public ObservableCollection<GlobalIdentifierViewModel> GlobalIdentifiers { get; private set; }

        public GlobalIdentifierViewModel SelectedGID { get; set; }

        public GetValues()
        {
            InitializeComponent();
            DataContext = this;

            GlobalIdentifiers = new ObservableCollection<GlobalIdentifierViewModel>();

            try
            {
                tgda = new TestGda();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "GetValues", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            foreach (DMSType dmsType in Enum.GetValues(typeof(DMSType)))
            {
                if (dmsType == DMSType.MASK_TYPE)
                {
                    continue;
                }

                ModelCode dmsTypesModelCode = modelResourcesDesc.GetModelCodeFromType(dmsType);
                tgda.GetExtentValues(dmsTypesModelCode, new List<ModelCode> { ModelCode.IDOBJ_GID }, null).ForEach(g => GlobalIdentifiers.Add(new GlobalIdentifierViewModel()
                {
                    GID = g,
                    Type = dmsTypesModelCode.ToString(),
                }));
            }

            SelectedGID = null;
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(SelectedGID == null)
            {
                return;
            }

            Properties.Children.Clear();

            Label label = new Label()
            {
                FontWeight = FontWeights.UltraBold,
                Content = "Properties(for selected entity)",
            };
            Properties.Children.Add(label);

            short type = ModelCodeHelper.ExtractTypeFromGlobalId(SelectedGID.GID);
            List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds((DMSType)type);

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
            if ((sender as CheckBox).IsChecked == false)
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

        private void ButtonGetValues_Click(object sender, RoutedEventArgs e)
        {
            if(SelectedGID == null)
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
            ResourceDescription rd = null;
            try
            {
                rd = tgda.GetValues(SelectedGID.GID, selectedProperties);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "GetValues", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (rd != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Returned entity" + Environment.NewLine + Environment.NewLine);
                sb.Append($"Entity with gid: 0x{rd.Id:X16}" + Environment.NewLine);

                foreach (Property property in rd.Properties)
                {
                    switch (property.Type)
                    {
                        case PropertyType.Int64:
                            StringAppender.AppendLong(sb, property);
                            break;
                        case PropertyType.Float:
                            StringAppender.AppendFloat(sb, property);
                            break;
                        case PropertyType.String:
                            StringAppender.AppendString(sb, property);
                            break;
                        case PropertyType.Reference:
                            StringAppender.AppendReference(sb, property);
                            break;
                        case PropertyType.ReferenceVector:
                            StringAppender.AppendReferenceVector(sb, property);
                            break;

                        default:
                            sb.Append($"{property.Id}: {property.PropertyValue.LongValue}{Environment.NewLine}");
                            break;
                    }
                }

                Values.Document.Blocks.Clear();
                Values.AppendText(sb.ToString());
            }
        }

        private void ButtonRefreshGids_Click(object sender, RoutedEventArgs e)
        {
            GlobalIdentifiers.Clear();

            foreach (DMSType dmsType in Enum.GetValues(typeof(DMSType)))
            {
                if (dmsType == DMSType.MASK_TYPE)
                {
                    continue;
                }

                ModelCode dmsTypesModelCode = modelResourcesDesc.GetModelCodeFromType(dmsType);
                tgda.GetExtentValues(dmsTypesModelCode, new List<ModelCode> { ModelCode.IDOBJ_GID }, null).ForEach(g => GlobalIdentifiers.Add(new GlobalIdentifierViewModel()
                {
                    GID = g,
                    Type = dmsTypesModelCode.ToString(),
                }));
            }

            SelectedGID = null;
        }
    }
}
