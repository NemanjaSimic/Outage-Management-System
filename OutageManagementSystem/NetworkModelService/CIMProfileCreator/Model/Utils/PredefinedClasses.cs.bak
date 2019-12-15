using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using FTN.ESI.SIMES.CIM.Manager;

namespace FTN.ESI.SIMES.CIM.Model.Utils
{
    public class PredefinedClasses
    {
        private Dictionary<string, List<NameValuePair>> predefinedPackages;
        private Dictionary<string, List<PredefinedProperty>> predifinedClasses;
        private Dictionary<string, List<string>> predifinedEnumerations;

        public PredefinedClasses()
        {
            readPredifinedPackages();
            readPredifinedClasses();
            readPredifinedEnumerations();
        }


        public List<string> PedifinedClassesList
        {
            get
            {
                return this.predifinedClasses.Keys.ToList<string>();
            }
        }


        private void readPredifinedPackages()
        {
            predefinedPackages = new Dictionary<string, List<NameValuePair>>();
            FileStream stream = new FileStream(".\\Predefined\\PredefinedPackages.xml", FileMode.Open);
            XmlTextReader reader = new XmlTextReader(stream);
            string key = string.Empty;
            List<NameValuePair> values = new List<NameValuePair>();

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name.Equals("Package"))
                    {
                        values.Clear();
                        if (reader.HasAttributes)
                        {
                            reader.MoveToFirstAttribute();
                            do
                            {
                                key = reader.Value;
                            } while (reader.MoveToNextAttribute());
                        }
                    }

                    if (reader.Name.Equals("URI"))
                    {
                        if (reader.HasAttributes)
                        {
                            reader.MoveToFirstAttribute();
                            do
                            {

                                values.Add(new NameValuePair("URI", reader.Value));
                            } while (reader.MoveToNextAttribute());
                        }
                    }

                    if (reader.Name.Equals("BelongsToCategory"))
                    {
                        if (reader.HasAttributes)
                        {
                            reader.MoveToFirstAttribute();
                            do
                            {
                                values.Add(new NameValuePair("BelongsToCategory", reader.Value));
                            } while (reader.MoveToNextAttribute());
                        }
                    }

                    if (reader.Name.Equals("Comment"))
                    {
                        if (reader.HasAttributes)
                        {
                            reader.MoveToFirstAttribute();
                            do
                            {
                                values.Add(new NameValuePair("Comment", reader.Value));
                            } while (reader.MoveToNextAttribute());
                        }
                    }

                    if (reader.Name.Equals("Label"))
                    {
                        if (reader.HasAttributes)
                        {
                            reader.MoveToFirstAttribute();
                            do
                            {
                                values.Add(new NameValuePair("Label", reader.Value));
                            } while (reader.MoveToNextAttribute());
                        }
                    }

                    if (reader.Name.Equals("Type"))
                    {
                        if (reader.HasAttributes)
                        {
                            reader.MoveToFirstAttribute();
                            do
                            {
                                values.Add(new NameValuePair("Type", reader.Value));
                            } while (reader.MoveToNextAttribute());
                        }
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name.Equals("Package"))
                    {
                        predefinedPackages.Add(key, values);
                    }
                }
            }


        }

        private void readPredifinedClasses()
        {
            predifinedClasses = new Dictionary<string, List<PredefinedProperty>>();
            FileStream stream = new FileStream(".\\Predefined\\PredefinedClasses.xml", FileMode.Open);
            XmlTextReader reader = new XmlTextReader(stream);
            string key = string.Empty;
            string uri = string.Empty;
            string type = string.Empty;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "DataType")
                    {
                        if (reader.HasAttributes)
                        {
                            reader.MoveToFirstAttribute();
                            do
                            {
                                key = reader.Value;
                            } while (reader.MoveToNextAttribute());
                        }
                    }

                    if (reader.Name == "URI")
                    {
                        if (reader.HasAttributes)
                        {
                            reader.MoveToFirstAttribute();
                            do
                            {
                                uri = reader.Value;
                            } while (reader.MoveToNextAttribute());
                        }
                    }

                    if (reader.Name == "Type")
                    {
                        if (reader.HasAttributes)
                        {
                            reader.MoveToFirstAttribute();
                            do
                            {
                                type = reader.Value;
                                List<PredefinedProperty> list;
                                predifinedClasses.TryGetValue(key, out list);
                                if (list == null)
                                {
                                    list = new List<PredefinedProperty>();
                                    predifinedClasses.Add(key, list);
                                }
                                list.Add(new PredefinedProperty(uri, type));
                            } while (reader.MoveToNextAttribute());
                        }
                    }
                }
            }
        }

        private void readPredifinedEnumerations()
        {
            predifinedEnumerations = new Dictionary<string, List<string>>();
            StreamReader st = new StreamReader(".\\Predefined\\PredefinedEnumerations.xml", Encoding.UTF8);
            XmlTextReader reader = new XmlTextReader(st);
            string key = string.Empty;
            string enume = string.Empty;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "EnumerationType")
                    {
                        if (reader.HasAttributes)
                        {
                            reader.MoveToFirstAttribute();
                            do
                            {
                                key = reader.Value;
                            } while (reader.MoveToNextAttribute());
                        }
                    }
                    if (reader.Name == "enum")
                    {
                        if (reader.HasAttributes)
                        {
                            reader.MoveToFirstAttribute();
                            do
                            {
                                enume = reader.Value;
                                //if (enume.Contains("DEGREE"))
                                //{
                                //    char c = (char)176;
                                //    enume = enume.Replace("DEGREE", c.ToString());
                                //}
                                List<string> list;
                                predifinedEnumerations.TryGetValue(key, out list);
                                if (list == null)
                                {
                                    list = new List<string>();
                                    predifinedEnumerations.Add(key, list);
                                }
                                list.Add(enume);
                            } while (reader.MoveToNextAttribute());
                        }
                    }
                }
            }
        }

        public void CreatePackage(Profile profile, string key)
        {
            List<NameValuePair> packageProps = predefinedPackages[key];
            ClassCategory package = new ClassCategory();
            foreach (NameValuePair prop in packageProps)
            {
                if (prop.Name.Equals("URI"))
                {
                    package.URI = prop.Value;
                }
                else if (prop.Name.Equals("BelongsToCategory"))
                {
                    package.BelongsToCategory = prop.Value;
                }
                else if (prop.Name.Equals("Comment"))
                {
                    package.Comment = prop.Value;
                }
                else if (prop.Name.Equals("Label"))
                {
                    package.Label = prop.Value;
                }
                else if (prop.Name.Equals("Type"))
                {
                    package.Type = prop.Value;
                }
            }

            List<ProfileElement> elems = profile.ProfileMap[ProfileElementTypes.ClassCategory];
            if (elems == null)
            {
                elems = new List<ProfileElement>();
            }

            if (elems.Count > 0 && !elems.Contains(package))
            {
                elems.Add(package);
                profile.ProfileMap.Remove(ProfileElementTypes.ClassCategory);
                profile.ProfileMap.Add(ProfileElementTypes.ClassCategory, elems);
            }

            package.BelongsToCategoryAsObject = (ClassCategory)profile.FindProfileElementByName(StringManipulationManager.ExtractAllAfterSeparator(package.BelongsToCategory, StringManipulationManager.SeparatorSharp));
            ((ClassCategory)profile.FindProfileElementByName(StringManipulationManager.ExtractAllAfterSeparator(package.BelongsToCategory, StringManipulationManager.SeparatorSharp))).AddToMembersOfClassCategory(package);

        }

        public void CreateEnumeration(Profile profile, string key)
        {
            Class enumeration = new Class();
            enumeration.Type = "Class";
            profile.ProfileMap[ProfileElementTypes.Class].Add(enumeration);
            List<string> enums;
            predifinedEnumerations.TryGetValue(key, out enums);
            if (enums != null)
            {
                foreach (string en in enums)
                {
                    EnumMember enumerationMember = new EnumMember();
                    //enumerationMember = new EnumMember();
                    enumerationMember.URI = "#" + key + "." + en;
                    enumerationMember.Label = en;
                    enumerationMember.EnumerationObject = enumeration;
                    enumeration.AddToMyEnumerationMembers(enumerationMember);
                }
                enumeration.URI = "#" + key;
                enumeration.BelongsToCategory = "#Package_Core";
                enumeration.BelongsToCategoryAsObject = profile.FindProfileElementByName(StringManipulationManager.ExtractAllAfterSeparator(enumeration.BelongsToCategory, StringManipulationManager.SeparatorSharp));
                ((ClassCategory)profile.FindProfileElementByName(StringManipulationManager.ExtractAllAfterSeparator(enumeration.BelongsToCategory, StringManipulationManager.SeparatorSharp))).AddToMembersOfClassCategory(enumeration);
            }

        }

        public void updateClassData(Class entity, Profile profile)
        {

            Property property;
            foreach (KeyValuePair<string, List<PredefinedProperty>> oneClass in predifinedClasses)
            {
                if (entity.Name.Equals(oneClass.Key))
                {
                    entity.BelongsToCategory = "#Package_Domain";
                    foreach (PredefinedProperty prop in oneClass.Value)
                    {
                        property = new Property();
                        property.URI = prop.URI;
                        property.DataType = prop.type;
                        property.Label = prop.URI.Split('.')[1];
                        if (prop.type.Equals("#UnitSymbol"))
                        {
                            property.Range = "#UnitSymbol";
                            property.RangeAsObject = profile.FindProfileElementByUri("#UnitSymbol");
                        }
                        if (prop.type.Equals("#UnitMultiplier"))
                        {
                            property.Range = "#UnitMultiplier";
                            property.RangeAsObject = profile.FindProfileElementByUri("#UnitMultiplier");
                        }

                        entity.AddToMyProperties(property);
                    }
                }
            }
        }
    }
}
