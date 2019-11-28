using Outage.Common;
using Outage.Common.GDA;
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

namespace Client.UserControls
{
    /// <summary>
    /// Interaction logic for GetValues.xaml
    /// </summary>
    public partial class GetValues : UserControl
    {
        public ObservableCollection<long> Gids { get; set; } = new ObservableCollection<long>();
        public ObservableCollection<ModelCode> Properties { get; set; } = new ObservableCollection<ModelCode>();
        public ModelResourcesDesc resourcesDesc { get; set; } = new ModelResourcesDesc();
        TestGda testGda = new TestGda();
        public GetValues()
        {
            InitializeComponent();
            DataContext = this;
            List<ModelCode> props = new List<ModelCode>();
            props.Add(ModelCode.IDOBJ_GID);

            //testGda.GetExtentValues(ModelCode.IDOBJ, props).ForEach(rd => Gids.Add(rd.Id));
            foreach (ModelCode modelcode in resourcesDesc.NonAbstractClassIds)
            {
                testGda.GetExtentValues(modelcode, props).ForEach(rd => Gids.Add(rd.Id));
            }


        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId((long)selectIDComboBox.SelectedItem);

            List<ModelCode> properties = resourcesDesc.GetAllPropertyIds(type);

            Properties.Clear();
            properties.ForEach(property => Properties.Add(property));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            List<ModelCode> selectedProperties = new List<ModelCode>();
            for (int i = 0; i < propertyListView.SelectedItems.Count; i++)
            {
                selectedProperties.Add((ModelCode)propertyListView.SelectedItems[i]);
            }

            ResourceDescription selectedItem = testGda.GetValues((long)selectIDComboBox.SelectedItem, selectedProperties);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"\tEntity: ").AppendLine($"Gid: {selectedItem.Id}");
            foreach (Property property in selectedItem.Properties)
            {
                if (property.Type != PropertyType.ReferenceVector)
                {
                    sb.AppendLine($"{property.Id.ToString()} : {property.GetValue().ToString()}");
                }
                else if (property.Type == PropertyType.Reference)
                {
                    sb.AppendLine($"{property.Id.ToString()} : {property.GetValue().ToString()}");
                }
                else
                {
                    sb.Append($"{property.Id.ToString()} : ");
                    foreach (long gid in property.AsReferences())
                    {
                        sb.Append($"{gid} ");
                    }
                    sb.AppendLine();
                }
            }

            infoTextBox.Text = sb.ToString();
            sb.Clear();
        }
    }
}
