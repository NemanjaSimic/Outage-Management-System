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
    /// Interaction logic for GetRelatedValues.xaml
    /// </summary>
    public partial class GetRelatedValues : UserControl
    {
        //public ObservableCollection<long> Gids { get; set; } = new ObservableCollection<long>();
        //public ObservableCollection<ModelCode> Properties { get; set; } = new ObservableCollection<ModelCode>();
        //public ObservableCollection<ModelCode> Associations { get; set; } = new ObservableCollection<ModelCode>();
        //public ObservableCollection<ModelCode> AssociationsTypes { get; set; } = new ObservableCollection<ModelCode>();
        //public ModelResourcesDesc resourcesDesc { get; set; } = new ModelResourcesDesc();
        //TestGda testGda = new TestGda();
        //public GetRelatedValues()
        //{
        //    InitializeComponent();
        //    DataContext = this;
        //    List<ModelCode> props = new List<ModelCode>();
        //    props.Add(ModelCode.IDOBJ_GID);

        //    testGda.GetExtentValues(ModelCode.CONNECTIVITYNODE, props).ForEach(rd => Gids.Add(rd.Id));
        //    testGda.GetExtentValues(ModelCode.TERMINAL, props).ForEach(rd => Gids.Add(rd.Id));
        //    testGda.GetExtentValues(ModelCode.SERIESCOMPENSATOR, props).ForEach(rd => Gids.Add(rd.Id));
        //    testGda.GetExtentValues(ModelCode.BAY, props).ForEach(rd => Gids.Add(rd.Id));
        //    testGda.GetExtentValues(ModelCode.DCLINESEGMENT, props).ForEach(rd => Gids.Add(rd.Id));
        //    testGda.GetExtentValues(ModelCode.ACLINESEGMENT, props).ForEach(rd => Gids.Add(rd.Id));

        //}

        //private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    infoTextBox.Clear();
        //    DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId((long)selectIDComboBox.SelectedItem);

        //    Associations.Clear();

        //    List<ModelCode> ref1 = resourcesDesc.GetPropertyIds(type, PropertyType.Reference);
        //    List<ModelCode> ref2 = resourcesDesc.GetPropertyIds(type, PropertyType.ReferenceVector);

        //    ref1.ForEach(modelCode => Associations.Add(modelCode));
        //    ref2.ForEach(modelCode => Associations.Add(modelCode));
        //}
        //private void referenceListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    AssociationsTypes.Clear();

        //    if (selectIDComboBox.SelectedItem == null || referenceListView.SelectedItem == null)
        //    {
        //        return;
        //    }

        //    ModelCode referencedType = resourcesDesc.ReferenceModelCodeMapping[(ModelCode)referenceListView.SelectedItem];

        //    if (resourcesDesc.NonAbstractClassIds.Contains(referencedType))
        //    {
        //        AssociationsTypes.Add(referencedType);
        //    }
        //    else
        //    {
        //        List<DMSType> leaves = resourcesDesc.GetLeaves(referencedType);  
        //        foreach (DMSType leaf in leaves)
        //        {
        //            ModelCode leafModelCode = resourcesDesc.GetModelCodeFromType(leaf);
        //            AssociationsTypes.Add(leafModelCode);
        //        }
        //    }

        //    if (AssociationsTypes.Count > 1)
        //    {
        //        AssociationsTypes.Add(0);
        //    }

        //    //if (referenceListView.SelectedItem != null)
        //    //{
        //    //    Association association = new Association();
        //    //    association.PropertyId = (ModelCode)referenceListView.SelectedItem;
        //    //    association.Type = 0;

        //    //    List<ModelCode> properties = new List<ModelCode>();
        //    //    properties.Add((ModelCode)referenceListView.SelectedItem);

        //    //    ResourceDescription rd = testGda.GetValues((long)selectIDComboBox.SelectedItem, properties);

        //    //    if (rd.Properties[0].Type != PropertyType.ReferenceVector)
        //    //    {
        //    //        DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(rd.Properties[0].AsReference());
        //    //        ModelCode code = resourcesDesc.GetModelCodeFromType(type);
        //    //        Associations.Add(code);
        //    //    }
        //    //    else
        //    //    {
        //    //        foreach (long propertyId in rd.Properties[0].AsReferences())
        //    //        {
        //    //            DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(propertyId);
        //    //            ModelCode code = resourcesDesc.GetModelCodeFromType(type);
        //    //            if (!Associations.Contains(code))
        //    //            {
        //    //                Associations.Add(code);
        //    //            }
        //    //        }
        //    //    }
        //    //}
        //}

        //private void SelectAssociationTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    Properties.Clear();
        //    if (AssociationsTypes.Count == 0)
        //    {
        //        return;
        //    }

        //    if ((ModelCode)selectAssociationTypeComboBox.SelectedItem != 0)
        //    {
        //        foreach (ModelCode modelCode in resourcesDesc.GetAllPropertyIds((ModelCode)selectAssociationTypeComboBox.SelectedItem))
        //        {
        //            Properties.Add(modelCode);
        //        }
        //    }
        //    else
        //    {
        //        for (int i = 0; i < AssociationsTypes.Count; i++)
        //        {
        //            if (AssociationsTypes[i] == 0)
        //            {
        //                continue;
        //            }

        //            foreach (ModelCode modelCode in resourcesDesc.GetAllPropertyIds(AssociationsTypes[i]))
        //            {
        //                if (!Properties.Contains(modelCode))
        //                {
        //                    Properties.Add(modelCode);
        //                }
        //            }
        //        }
        //    }
        //}

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{

        //    if (selectIDComboBox.SelectedItem == null || selectAssociationTypeComboBox.SelectedItem == null || referenceListView == null)
        //    {
        //        MessageBox.Show("You must select gid, association and association type!");
        //        return;
        //    }

        //    List<ModelCode> propertiesForRefType = new List<ModelCode>();
        //    propertiesForRefType.Add((ModelCode)referenceListView.SelectedItem);

        //    ResourceDescription resourceDescription = testGda.GetValues((long)selectIDComboBox.SelectedItem, propertiesForRefType);
        //    List<ModelCode> referencedModelCodes = new List<ModelCode>();

        //    if (resourceDescription.Properties[0].Type == PropertyType.Reference)
        //    {
        //        if (resourceDescription.Properties[0].AsReference() != 0)
        //        {
        //            referencedModelCodes.Add(resourcesDesc.GetModelCodeFromId(resourceDescription.Properties[0].AsReference()));
        //        }
        //    }
        //    else
        //    {
        //        List<long> gids = resourceDescription.Properties[0].AsReferences();
        //        if (gids.Count != 0)
        //        {
        //            foreach (long gid in gids)
        //            {
        //                ModelCode modelCode = resourcesDesc.GetModelCodeFromId(gid);
        //                if (!referencedModelCodes.Contains(modelCode))
        //                {
        //                    referencedModelCodes.Add(modelCode);
        //                }
        //            }
        //        }
        //    }
        //    List<ModelCode> propertiesOfReferencedTypes = new List<ModelCode>();
        //    foreach (ModelCode referencedModelCode in referencedModelCodes)
        //    {
        //        List<ModelCode> propertiesOfReferencedType = resourcesDesc.GetAllPropertyIds(referencedModelCode);
        //        foreach (ModelCode propertyOfReferencedType in propertiesOfReferencedType)
        //        {
        //            if (!propertiesOfReferencedTypes.Contains(propertyOfReferencedType))
        //            {
        //                propertiesOfReferencedTypes.Add(propertyOfReferencedType);
        //            }
        //        }
        //    }

        //    List<ModelCode> properties = new List<ModelCode>();

        //    foreach (var element in propertyListView.SelectedItems)
        //    {
        //        if (propertiesOfReferencedTypes.Contains((ModelCode)element))
        //        {
        //            properties.Add((ModelCode)element);
        //        }
        //    }

        //    Association association = new Association((ModelCode)referenceListView.SelectedItem, (ModelCode)selectAssociationTypeComboBox.SelectedItem);

        //    List<ResourceDescription> selectedRds = testGda.GetRelatedValues((long)selectIDComboBox.SelectedItem, association, properties);

        //    StringBuilder sb = new StringBuilder();

        //    //List<ModelCode> selectedProperties = new List<ModelCode>();
        //    //for (int i = 0; i < propertyListView.SelectedItems.Count; i++)
        //    //{
        //    //    selectedProperties.Add((ModelCode)propertyListView.SelectedItems[i]);
        //    //}

        //    //ResourceDescription selectedItem = testGda.GetValues((long)selectIDComboBox.SelectedItem, selectedProperties);

        //    //StringBuilder sb = new StringBuilder();

        //    sb.AppendLine($"Entities of related type: ").AppendLine($"Gid: {referenceListView.SelectedItem.ToString()}");

        //    foreach (ResourceDescription rd in selectedRds)
        //    {
        //        sb.AppendLine($"Entity gid: {rd.Id}");
        //        foreach (Property property in rd.Properties)
        //        {
        //            if (property.Type != PropertyType.ReferenceVector)
        //            {
        //                sb.AppendLine($"\t{property.Id.ToString()} : {property.GetValue().ToString()}");
        //            }
        //            else
        //            {
        //                sb.Append($"\t{property.Id.ToString()} : ");
        //                foreach (long gid in property.AsReferences())
        //                {
        //                    sb.Append($"{gid} ");
        //                }
        //                sb.AppendLine();
        //            }

        //        }
        //    }
        //    //foreach (Property property in selectedItem.Properties)
        //    //{
        //    //    if (property.Type != PropertyType.ReferenceVector)
        //    //    {
        //    //        sb.AppendLine($"{property.Id.ToString()} : {property.GetValue().ToString()}");
        //    //    }
        //    //    else if (property.Type == PropertyType.Reference)
        //    //    {
        //    //        sb.AppendLine($"{property.Id.ToString()} : {property.GetValue().ToString()}");
        //    //    }
        //    //    else
        //    //    {
        //    //        sb.Append($"{property.Id.ToString()} : ");
        //    //        foreach (long gid in property.AsReferences())
        //    //        {
        //    //            sb.Append($"{gid} ");
        //    //        }
        //    //        sb.AppendLine();
        //    //    }
        //    //}

        //    infoTextBox.Text = sb.ToString();
        //    sb.Clear();
        //}


    }
}
