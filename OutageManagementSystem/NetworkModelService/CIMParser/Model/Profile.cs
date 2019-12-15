using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using CIM.Manager;

namespace CIM.Model
{
    /// <summary>
    /// Profile class represents the CIM profile loaded from RDFS source file.
    /// <para>@author: Stanislava Selena</para>
    /// </summary>
    public class Profile
    {
        /// <summary> 
        /// List of common stereotypes for profile elements which can be extended
        /// by adding newly identified stereotypes in profile processing process.
        /// <para>See also: <seealso cref="M:FindOrCreateStereotypeForName(string fullStereotypeName)"/></para>
        /// </summary>
        public static List<ProfileElementStereotype> StereotypeList;
        

        private SortedDictionary<ProfileElementTypes, List<ProfileElement>> profileMap;
        private string sourcePath;
        private string fileName;
        private string baseNS;
        private double fileSizeMB = 0;
        private DateTime? lastModificationTime;

        private bool loaderErrorOcurred = false;


        static Profile()
        {
            StereotypeList = new List<ProfileElementStereotype>();
            // init common stereotypes list
            int index = 0;
            StereotypeList.Add(new ProfileElementStereotype(index++, ProfileElementStereotype.StereotypeConcrete));
            StereotypeList.Add(new ProfileElementStereotype(index++, ProfileElementStereotype.StereotypeCompound));
            StereotypeList.Add(new ProfileElementStereotype(index++, ProfileElementStereotype.StereotypeAttribute));
            StereotypeList.Add(new ProfileElementStereotype(index++, ProfileElementStereotype.StereotypeEnumeration));
            StereotypeList.Add(new ProfileElementStereotype(index++, ProfileElementStereotype.StereotypeByReference));
            StereotypeList.Add(new ProfileElementStereotype(index++, ProfileElementStereotype.StereotypeAggregateOf));
            StereotypeList.Add(new ProfileElementStereotype(index++, ProfileElementStereotype.StereotypeOfAggregate));
            StereotypeList.Add(new ProfileElementStereotype(index++, ProfileElementStereotype.StereotypeCompositeOf));
        }

        
        public Profile()
        {
            profileMap = new SortedDictionary<ProfileElementTypes, List<ProfileElement>>();
        }

        public Profile(string sourcePath)
        {
            profileMap = new SortedDictionary<ProfileElementTypes, List<ProfileElement>>();
            SourcePath = sourcePath;
        }

        /// <summary>
        /// Gets and sets the map which projectModels the profile.        
        /// </summary>
        public SortedDictionary<ProfileElementTypes, List<ProfileElement>> ProfileMap 
        {
            get
            {
                return profileMap;
            }
            set 
            {
                profileMap = value;                
                SortElementsInMap();                
            }
        }

        /// <summary>
        /// Gets and sets the absolute file path to source file of profile.
        /// </summary>
        public string SourcePath
        {
            set 
            {
                sourcePath = value;
                fileName = string.Empty;
                fileSizeMB = 0;
                if (sourcePath != null)
                {
                    fileName = System.IO.Path.GetFileName(sourcePath);
                    lastModificationTime = FileManager.ReadLastModificationTimeForFile(sourcePath);
                    fileSizeMB = FileManager.ReadFileSizeInMBForFile(sourcePath);
                }                
            }
            get
            {
                return sourcePath;
            }
        }

        /// <summary>
        /// Gets the file name of profile's source file (with extension).
        /// </summary>
        public string FileName
        {
            get
            {
                return fileName;
            }
        }

        public string BaseNS 
        {
            set
            {
                baseNS = value;
            }

            get
            {
                return baseNS;
            }
        }


        /// <summary>
        /// Gets the size of profile's source file (in MB).
        /// </summary>
        public double FileSizeMB
        {
            get
            {
                return fileSizeMB;
            }
        }

        /// <summary>
        /// Gets and sets the time of last modification of source file.
        /// </summary>
        public DateTime? LastModificationTime
        {
            get
            {
                return lastModificationTime;
            }
            set
            {
                lastModificationTime = value;
            }
        }

        /// <summary>
        /// Get and sets the indication if there was serious errors during initial loading of project file.
        /// <para>This error is reported by LoaderManager's LoadCIMProject() method.</para>
        /// </summary>
        public bool LoaderErrorOcurred
        {
            get
            {
                return loaderErrorOcurred;
            }
            set
            {
                loaderErrorOcurred = value;
                if (loaderErrorOcurred && (profileMap != null))
                {
                    profileMap.Clear();
                }
            }
        }

        /// <summary>
        /// Gets the number of all ProfileElementTypes.ClassCategory in this profile.
        /// </summary>
        public int PackageCount
        {
            get
            {
                int count = 0;
                if (profileMap != null)
                {
                    List<ProfileElement> elements = null;
                    profileMap.TryGetValue(ProfileElementTypes.ClassCategory, out elements);
                    if (elements != null)
                    {
                        count = elements.Count;
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// Gets the number of all ProfileElementTypes.Class in this profile.
        /// </summary>
        public int ClassCount
        {
            get
            {
                int count = 0;
                if (profileMap != null)
                {
                    List<ProfileElement> elements = null;
                    profileMap.TryGetValue(ProfileElementTypes.Class, out elements);
                    if (elements != null)
                    {
                        count = elements.Count;
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// Gets the number of all ProfileElementTypes.Property in this profile.
        /// </summary>
        public int PropertyCount
        {
            get
            {
                int count = 0;
                if (profileMap != null)
                {
                    List<ProfileElement> elements = null;
                    profileMap.TryGetValue(ProfileElementTypes.Property, out elements);
                    if (elements != null)
                    {
                        count = elements.Count;
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// Gets the number of all ProfileElementTypes.Stereotype in this profile.
        /// </summary>
        public int StereotypeCount
        {
            get
            {
                int count = 0;
                if (profileMap != null)
                {
                    List<ProfileElement> elements = null;
                    profileMap.TryGetValue(ProfileElementTypes.Stereotype, out elements);
                    if (elements != null)
                    {
                        count = elements.Count;
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// Method first tries to find the ProfileElementStereotype object with given fullName isInside the
        /// (static) StereotypeList, and if it doesn't find it, it creates new stereotype objects 
        /// and adds it to this list.
        /// <remarks>If the fullStereotypeName is null, method will return null.</remarks>
        /// </summary>
        /// <param fullName="fullStereotypeName">full fullName of stereotype which is being searched</param>
        /// <returns>ProfileElementStereotype object with given fullName founded (or added) in StereotypeList</returns>
        public static ProfileElementStereotype FindOrCreateStereotypeForName(string fullStereotypeName) 
        {
            ProfileElementStereotype stereotype = null;
            if (!string.IsNullOrEmpty(fullStereotypeName))
            {
                string shortName = StringManipulationManager.ExtractShortestName(fullStereotypeName, StringManipulationManager.SeparatorSharp);

                foreach (ProfileElementStereotype existingStereotype in Profile.StereotypeList)
                {
                    if (existingStereotype.Name.Equals(fullStereotypeName) || existingStereotype.Name.Equals(shortName))
                    {
                        stereotype = existingStereotype;
                        break;
                    }
                }

                if (stereotype == null)
                {
                    stereotype = new ProfileElementStereotype(Profile.StereotypeList.Count, fullStereotypeName);
                    Profile.StereotypeList.Add(stereotype);
                }
            }
            return stereotype;
        }


        public List<ProfileElement> GetAllProfileElementsOfType(ProfileElementTypes type) {
            List<ProfileElement> elementsOfType = new List<ProfileElement>();
            if (profileMap != null)
            {
                profileMap.TryGetValue(type, out elementsOfType);
            }
            return elementsOfType;
        }


        public ProfileElement FindProfileElementByUri(string uri)
        {
            ProfileElement element = null;

            if (!string.IsNullOrEmpty(uri) && (profileMap != null))
            {
                foreach (ProfileElementTypes type in profileMap.Keys) {
                    List<ProfileElement> list = profileMap[type];
                    foreach (ProfileElement elem in list) 
                    {
                        if (uri.Equals(elem.URI))
                        {
                            element = elem;
                            break;
                        }
                    }
                    if (element != null)
                    {
                        break;
                    }
                }                
            }
            return element;
        }

        public ProfileElement FindProfileElementByShortUri(string shortUri)
        {
            ProfileElement element = null;

            if (!string.IsNullOrEmpty(shortUri) && (profileMap != null))
            {
                foreach (ProfileElementTypes type in profileMap.Keys)
                {
                    List<ProfileElement> list = profileMap[type];
                    foreach (ProfileElement elem in list)
                    {
                        if (shortUri.Equals(elem.UniqueName))
                        {
                            element = elem;
                            break;
                        }
                    }
                    if (element != null)
                    {
                        break;
                    }
                }
            }
            return element;
        }

        public ProfileElement FindProfileElementByName(string name)
        {
            ProfileElement element = null;

            if (!string.IsNullOrEmpty(name) && (profileMap != null))
            {
                foreach (ProfileElementTypes type in profileMap.Keys)
                {
                    List<ProfileElement> list = profileMap[type];
                    foreach (ProfileElement elem in list)
                    {
                        if (name.Equals(elem.Name))
                        {
                            element = elem;
                            break;
                        }
                    }
                    if (element != null)
                    {
                        break;
                    }
                }
            }
            return element;
        }

        public string FullPrintingString()
        {
            StringBuilder toStringBuilder = new StringBuilder("Profile: \n");
            if (profileMap != null)
            {
                if (profileMap.ContainsKey(ProfileElementTypes.ClassCategory))
                {
                    foreach (ProfileElement package in profileMap[ProfileElementTypes.ClassCategory])
                    {
                        toStringBuilder.Append("* members of ");
                        toStringBuilder.AppendLine(package.UniqueName);
                        List<ProfileElement> list = package.MembersOfClassCategory;
                        if (list != null)
                        {
                            foreach (ProfileElement elem in list)
                            {
                                if (elem.TypeAsEnumValue == ProfileElementTypes.Class)
                                {
                                    toStringBuilder.Append("\t ");
                                    toStringBuilder.AppendLine(elem.URI);
                                    toStringBuilder.Append("\t\t type = ");
                                    toStringBuilder.AppendLine(elem.TypeAsEnumValue.ToString());
                                    toStringBuilder.Append("\t\t label = ");
                                    toStringBuilder.AppendLine(elem.Label);

                                    if (elem.Stereotypes != null)
                                    {
                                        toStringBuilder.AppendLine("\t\t has Stereotypes : ");
                                        foreach (ProfileElementStereotype stereotype in elem.Stereotypes)
                                        {
                                            toStringBuilder.Append("\t\t\t");
                                            toStringBuilder.AppendLine(stereotype.ToString());
                                        }
                                    }

                                    toStringBuilder.Append("\t\t subClassOf = ");
                                    toStringBuilder.AppendLine(elem.SubClassOf);
                                    toStringBuilder.Append("\t\t belongsToCategory = ");
                                    toStringBuilder.AppendLine(elem.BelongsToCategory);
                                    if (elem.MyProperties != null)
                                    {
                                        toStringBuilder.AppendLine("\t\t has Properties : ");
                                        foreach (ProfileElement property in elem.MyProperties)
                                        {
                                            toStringBuilder.Append("\t\t\t");
                                            toStringBuilder.Append(property.UniqueName);
                                            toStringBuilder.Append(" ( ");
                                            toStringBuilder.Append(property.URI);
                                            toStringBuilder.AppendLine(" )");
                                            toStringBuilder.Append("\t\t\t\t label = ");
                                            toStringBuilder.AppendLine(property.Label);
                                            toStringBuilder.Append("\t\t\t\t dataType = ");
                                            toStringBuilder.AppendLine(property.DataType);
                                            toStringBuilder.Append("\t\t\t\t range = ");
                                            toStringBuilder.AppendLine(property.Range);
                                            toStringBuilder.Append("\t\t\t\t multiplicity = ");
                                            toStringBuilder.AppendLine(property.Multiplicity.ToString());
                                            toStringBuilder.Append("\t\t\t\t inverseRoleName = ");
                                            toStringBuilder.AppendLine(property.InverseRoleName);

                                            if (property.Stereotypes != null)
                                            {
                                                toStringBuilder.AppendLine("\t\t\t\t has Stereotypes : ");
                                                foreach (ProfileElementStereotype stereotype in property.Stereotypes)
                                                {
                                                    toStringBuilder.Append("\t\t\t\t\t");
                                                    toStringBuilder.AppendLine(stereotype.ToString());
                                                }
                                            }

                                            toStringBuilder.AppendLine();
                                        }
                                    }

                                    if (elem.MyEnumerationMembers != null)
                                    {
                                        toStringBuilder.AppendLine("\t\t\t has enum members : ");
                                        foreach (ProfileElement enumMember in elem.MyEnumerationMembers)
                                        {
                                            toStringBuilder.Append("\t\t\t\t");
                                            toStringBuilder.AppendLine(enumMember.UniqueName);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return toStringBuilder.ToString();
        }

        private void SortElementsInMap()
        {
            if ((profileMap != null) && (profileMap.Count > 0))
            {                
                foreach (ProfileElementTypes profileType in profileMap.Keys)
                {                    
                    if ((profileMap[profileType] != null) && (profileMap[profileType].Count > 0))
                    {
                        profileMap[profileType].Sort(CIMComparer.ProfileElementComparer);
                    }                    
                }
            }
        }
    }
}
