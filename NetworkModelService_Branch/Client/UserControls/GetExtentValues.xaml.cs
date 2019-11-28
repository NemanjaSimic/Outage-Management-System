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
    /// Interaction logic for GetExtentValues.xaml
    /// </summary>
    public partial class GetExtentValues : UserControl
    {
        TestGda testGda = new TestGda();
        public ObservableCollection<DMSType> Types { get; set; } = new ObservableCollection<DMSType>();
        public ObservableCollection<ModelCode> Properties { get; set; } = new ObservableCollection<ModelCode>();
        public ModelResourcesDesc ResourcesDesc { get; set; } = new ModelResourcesDesc();
        public GetExtentValues()
        {
            InitializeComponent();
            DataContext = this;

            Array types = Enum.GetValues(typeof(DMSType));

            foreach (DMSType type in types)
            {
                if (type == DMSType.MASK_TYPE)
                {
                    continue;
                }
                Types.Add(type);
            }
        }

        private void selectTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<ModelCode> properties = ResourcesDesc.GetAllPropertyIds((DMSType)selectTypeComboBox.SelectedItem);
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
            ModelCode selectedType = ResourcesDesc.GetModelCodeFromType((DMSType)selectTypeComboBox.SelectedItem);
            List<ResourceDescription> selectedRds = testGda.GetExtentValues(selectedType, selectedProperties);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Entities of type: {selectedType.ToString()}");

            foreach (ResourceDescription rd in selectedRds)
            {
                sb.AppendLine($"Entity gid: {rd.Id}");
                foreach (Property property in rd.Properties)
                {
                    if (property.Type != PropertyType.ReferenceVector)
                    {
                        sb.AppendLine($"\t{property.Id.ToString()} : {property.GetValue().ToString()}");
                    }
                    else
                    {
                        sb.Append($"\t{property.Id.ToString()} : ");
                        foreach (long gid in property.AsReferences())
                        {
                            sb.Append($"{gid} ");
                        }
                        sb.AppendLine();
                    }

                }
                sb.AppendLine("================================").AppendLine();
            }

            infoTextBox.Text = sb.ToString();

            sb.Clear();
        }
    }
}
