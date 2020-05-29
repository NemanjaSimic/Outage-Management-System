using FTN.Services.NetworkModelService.TestClientUI;
using OMS.Common.NmsContracts.GDA;
using Outage.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TelventDMS.Services.NetworkModelService.TestClient.TestsUI;

namespace NMSTestClientUI.UserControls
{
    /// <summary>
    /// Interaction logic for GetRelatedValues.xaml
    /// </summary>
    public partial class GetRelatedValues : UserControl
    {
        private TestGda tgda;
        private ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
        private Dictionary<ModelCode, string> propertiesDesc = new Dictionary<ModelCode, string>();

        public ObservableCollection<GlobalIdentifierViewModel> GlobalIdentifiersRelated { get; private set; }
        public ObservableCollection<PropertyViewModel> RelationalProperties { get; private set; }
        public ObservableCollection<DmsTypeViewModel> RelatedEntityDmsTypes { get; set; }

        public GlobalIdentifierViewModel SelectedGID { get; set; }
        public PropertyViewModel SelectedProperty { get; set; }
        public DmsTypeViewModel SelectedDmsType { get; set; }

        public GetRelatedValues()
        {
            InitializeComponent();
            DataContext = this;

            GlobalIdentifiersRelated = new ObservableCollection<GlobalIdentifierViewModel>();
            RelationalProperties = new ObservableCollection<PropertyViewModel>();
            RelatedEntityDmsTypes = new ObservableCollection<DmsTypeViewModel>();

            try
            {
                tgda = new TestGda();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "GetRelatedValues", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Task.Run(Initialize);
        }

        private async void Initialize()
        {
            foreach (DMSType dmsType in Enum.GetValues(typeof(DMSType)))
            {
                if (dmsType == DMSType.MASK_TYPE)
                {
                    continue;
                }

                ModelCode dmsTypesModelCode = modelResourcesDesc.GetModelCodeFromType(dmsType);
                List<long> gids = await tgda.GetExtentValues(dmsTypesModelCode, new List<ModelCode> { ModelCode.IDOBJ_GID }, null);

                foreach (long gid in gids)
                {
                    Dispatcher.Invoke(() =>
                    {
                        GlobalIdentifiersRelated.Add(new GlobalIdentifierViewModel()
                        {
                            GID = gid,
                            Type = dmsTypesModelCode.ToString(),
                        });
                    });
                }
            }
        }

        private void GlobalIdentifiersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(SelectedGID == null)
            {
                return;
            }

            RelationalProperties.Clear();
            RelatedEntityDmsTypes.Clear();
            SelectedProperty = null;
            RelatedValues.Document.Blocks.Clear();
            PropertiesInRelated.Children.Clear();

            short type = ModelCodeHelper.ExtractTypeFromGlobalId(SelectedGID.GID);
            List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds((DMSType)type);


            foreach (ModelCode property in properties)
            {
                Property prop = new Property(property);
                if (prop.Type != PropertyType.Reference && prop.Type != PropertyType.ReferenceVector)
                {
                    continue;
                }

                RelationalProperties.Add(new PropertyViewModel() { Property = property });
            }
        }

        private void RelationalPropertiesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(SelectedProperty == null)
            {
                return;
            }

            RelatedEntityDmsTypes.Clear();
            SelectedDmsType = null;
            PropertiesInRelated.Children.Clear();
            RelatedValues.Document.Blocks.Clear();

            List<DMSType> dmsTypes = new List<DMSType>();
            if (RelationalPropertiesHelper.Relations.ContainsKey(SelectedProperty.Property))
            {
                ModelCode relatedEntity = RelationalPropertiesHelper.Relations[SelectedProperty.Property];
                dmsTypes.AddRange(ModelResourcesDesc.GetLeavesForCoreEntities(relatedEntity));

                if (dmsTypes.Count == 0)
                {
                    dmsTypes.Add(ModelCodeHelper.GetTypeFromModelCode(relatedEntity));
                }
            }

            foreach(DMSType type in dmsTypes)
            {
                RelatedEntityDmsTypes.Add(new DmsTypeViewModel() { DmsType = type });
            }

            HashSet<ModelCode> referencedTypeProperties = new HashSet<ModelCode>();
            if (RelatedEntityDmsTypes.Count > 0)
            {
                foreach (DmsTypeViewModel referencedDmsType in RelatedEntityDmsTypes)
                {
                    foreach (ModelCode propInReferencedType in modelResourcesDesc.GetAllPropertyIds(referencedDmsType.DmsType))
                    {
                        if (!referencedTypeProperties.Contains(propInReferencedType))
                        {
                            referencedTypeProperties.Add(propInReferencedType);
                        }
                    }
                }
            }

            Label label = new Label()
            {
                FontWeight = FontWeights.UltraBold,
                Content = "Properties (for classes in selected relation)",
            };
            PropertiesInRelated.Children.Add(label);

            propertiesDesc.Clear();

            if (referencedTypeProperties.Count > 0)
            {
                foreach (ModelCode property in referencedTypeProperties)
                {
                    if (propertiesDesc.ContainsKey(property))
                    {
                        continue;
                    }

                    propertiesDesc.Add(property, property.ToString());

                    CheckBox checkBox = new CheckBox()
                    {
                        Content = property.ToString(),
                    };
                    checkBox.Unchecked += CheckBox_Unchecked;
                    PropertiesInRelated.Children.Add(checkBox);
                }
                CheckAllBtn.IsEnabled = true;
            }
        }

        private void RelatedEntityDmsTypesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(SelectedDmsType == null)
            {
                return;
            }

            PropertiesInRelated.Children.Clear();
            RelatedValues.Document.Blocks.Clear();

            HashSet<ModelCode> referencedTypeProperties = new HashSet<ModelCode>();
            foreach (ModelCode propInReferencedType in modelResourcesDesc.GetAllPropertyIds(SelectedDmsType.DmsType))
            {
                if (!referencedTypeProperties.Contains(propInReferencedType))
                {
                    referencedTypeProperties.Add(propInReferencedType);
                }
            }

            Label label = new Label()
            {
                FontWeight = FontWeights.UltraBold,
                Content = "Properties (for classes in selected relation)",
            };
            PropertiesInRelated.Children.Add(label);

            propertiesDesc.Clear();

            if (referencedTypeProperties.Count > 0)
            {
                foreach (ModelCode property in referencedTypeProperties)
                {
                    if (propertiesDesc.ContainsKey(property))
                    {
                        continue;
                    }

                    propertiesDesc.Add(property, property.ToString());

                    CheckBox checkBox = new CheckBox()
                    {
                        Content = property.ToString(),
                    };
                    checkBox.Unchecked += CheckBox_Unchecked;
                    PropertiesInRelated.Children.Add(checkBox);
                }
                    CheckAllBtn.IsEnabled = true;
            }
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

            foreach (var child in PropertiesInRelated.Children)
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

            foreach (var child in PropertiesInRelated.Children)
            {
                CheckBox checkBox;
                if (child is CheckBox)
                {
                    checkBox = child as CheckBox;
                    checkBox.IsChecked = false;
                }
            }
        }

        private async void ButtonGetRelatedValues_Click(object sender, RoutedEventArgs e)
        {
            if(SelectedProperty == null)
            {
                return;
            }

            GetRelatedValuesButton.IsEnabled = false;

            List<ModelCode> selectedProperties = new List<ModelCode>();

            foreach (var child in PropertiesInRelated.Children)
            {
                CheckBox checkBox;
                if (child is CheckBox && (child as CheckBox).IsChecked.Value)
                {
                    checkBox = child as CheckBox;
                    foreach (KeyValuePair<ModelCode, string> keyValuePair in propertiesDesc)
                    {
                        if (keyValuePair.Value.Equals(checkBox.Content))
                        {
                            selectedProperties.Add(keyValuePair.Key);
                        }
                    }
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("Returned entities" + Environment.NewLine + Environment.NewLine);

            ////////////////////////////////////////////
            List<long> gidReferences = new List<long>();
            ResourceDescription rd = await tgda.GetValues(SelectedGID.GID, new List<ModelCode>() { SelectedProperty.Property });
            if (rd != null)
            {
                Property prop = rd.GetProperty(SelectedProperty.Property);

                if ((short)(unchecked((long)SelectedProperty.Property & (long)ModelCodeMask.MASK_ATTRIBUTE_TYPE)) == (short)PropertyType.Reference)
                {
                    gidReferences.Add(prop.AsReference());
                }
                else if ((short)(unchecked((long)SelectedProperty.Property & (long)ModelCodeMask.MASK_ATTRIBUTE_TYPE)) == (short)PropertyType.ReferenceVector)
                {
                    gidReferences.AddRange(prop.AsReferences());
                }
            }

            HashSet<DMSType> referencedDmsTypes = new HashSet<DMSType>();
            if (gidReferences.Count > 0)
            {
                foreach (long gidReference in gidReferences)
                {
                    DMSType dmsType = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(gidReference);
                    if (!referencedDmsTypes.Contains(dmsType))
                    {
                        referencedDmsTypes.Add(dmsType);
                    }
                }
            }
            ////////////////////////////////////////////////////////

            try
            {
                if (SelectedDmsType != null)
                {
                    Association association = new Association(SelectedProperty.Property, modelResourcesDesc.GetModelCodeFromType(SelectedDmsType.DmsType));
                    List<long> gids = await tgda.GetRelatedValues(SelectedGID.GID, selectedProperties, association, sb);
                }
                else
                {
                    /////////////////////////////////////////////////////////////
                    HashSet<ModelCode> referencedDmsTypesProperties = new HashSet<ModelCode>(modelResourcesDesc.GetAllPropertyIds(referencedDmsTypes.First()));
                    List<ModelCode> toBeRemovedFormSelectedProperties = new List<ModelCode>();
                    foreach (ModelCode property in selectedProperties)
                    {
                        if(!referencedDmsTypesProperties.Contains(property))
                        {
                            toBeRemovedFormSelectedProperties.Add(property);
                        }
                    }

                    foreach(ModelCode property in toBeRemovedFormSelectedProperties)
                    {
                        selectedProperties.Remove(property);
                    }

                    foreach (var child in PropertiesInRelated.Children)
                    {
                        CheckBox checkBox;
                        if (child is CheckBox && (child as CheckBox).IsChecked.Value)
                        {
                            checkBox = child as CheckBox;
                            foreach (KeyValuePair<ModelCode, string> keyValuePair in propertiesDesc)
                            {
                                if (keyValuePair.Value.Equals(checkBox.Content) && toBeRemovedFormSelectedProperties.Contains(keyValuePair.Key))
                                {
                                    checkBox.IsChecked = false;
                                }
                            }
                        }
                    }

                    CheckAllBtn.IsEnabled = true;
                    /////////////////////////////////////////////////////////////

                    Association association = new Association(SelectedProperty.Property, 0x0000000000000000);
                    List<long> gids = await tgda.GetRelatedValues(SelectedGID.GID, selectedProperties, association, sb);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "GetRelatedValues", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            RelatedValues.Document.Blocks.Clear();
            RelatedValues.AppendText(sb.ToString());
            GetRelatedValuesButton.IsEnabled = true;
        }

        private async void ButtonRefreshGids_Click(object sender, RoutedEventArgs e)
        {
            RefreshButton.IsEnabled = false;

            GlobalIdentifiersRelated.Clear();

            foreach (DMSType dmsType in Enum.GetValues(typeof(DMSType)))
            {
                if (dmsType == DMSType.MASK_TYPE)
                {
                    continue;
                }

                ModelCode dmsTypesModelCode = modelResourcesDesc.GetModelCodeFromType(dmsType);
                List<long> gids = await tgda.GetExtentValues(dmsTypesModelCode, new List<ModelCode> { ModelCode.IDOBJ_GID }, null);
                
                foreach(long gid in gids)
                {
                    Dispatcher.Invoke(() =>
                    {
                        GlobalIdentifiersRelated.Add(new GlobalIdentifierViewModel()
                        {
                            GID = gid,
                            Type = dmsTypesModelCode.ToString(),
                        });
                    });
                }
            }

            SelectedGID = null;
            RefreshButton.IsEnabled = true;
        }
    }
}
