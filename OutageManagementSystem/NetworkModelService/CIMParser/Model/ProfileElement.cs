using System;
using System.Collections.Generic;
using System.ComponentModel;
using CIM.Manager;

namespace CIM.Model
{
    /// <summary>
    /// Possible types of profile elements
    /// </summary>
    public enum ProfileElementTypes
    {
        Unknown = 0, ClassCategory, Class, Property, EnumerationElement, Stereotype
    };

    /// <summary>
    /// Possible multiplicity of profile elements
    /// </summary>
    public enum ProfileElementMultiplicity
    {
        /// <summary> M:0..1 </summary>
        ZeroOrOne = 0,
        /// <summary> M:1 or M:1..1 </summary>
        ExactlyOne,
        /// <summary> M:0..n </summary>
        ZeroOrMore,
        /// <summary> M:1..n </summary>
        OneOrMore
    };

    /// <summary>
    /// ProfileElement class represents one element founded during processing of profile's source file.
    /// <para>See also: <seealso cref="T:Profile"/></para>
    /// <para>@author: Stanislava Selena</para>
    /// </summary>
    public class ProfileElement
    {
        public const string Separator = StringManipulationManager.SeparatorSharp;

        /// <summary> string with value "classcategory" </summary>
        public const string TypeClassCategoryString = "classcategory";
        /// <summary> string with value "class" </summary>
        public const string TypeClassString = "class";
        /// <summary> string with value "property" </summary>
        public const string TypePropertyString = "property";
        /// <summary> string with value "stereotype" </summary>
        public const string TypeStereotypeString = "stereotype";

        protected const string MultiplicityZeroOrOneString = "M:0..1";
        protected const string MultiplicityExactlyOneString1 = "M:1..1";
        protected const string MultiplicityExactlyOneString2 = "M:1";
        protected const string MultiplicityZeroOrMoreString = "M:0..n";
        protected const string MultiplicityOneOrMoreString = "M:1..n";

        /// <summary> "integer" </summary>
        protected const string SimpleDataTypeInteger = "integer";
        /// <summary> "int" </summary>
        protected const string SimpleDataTypeInt = "int";
        /// <summary> "float"  </summary>
        protected const string SimpleDataTypeFloat = "float";
        /// <summary> "string" </summary>
        protected const string SimpleDataTypeString = "string";
        /// <summary> "dateTime" </summary>
        protected const string SimpleDataTypeDateTime = "datetime";
        /// <summary> "boolean" </summary>
        protected const string SimpleDataTypeBoolean = "boolean";
        /// <summary> "bool" </summary>
        protected const string SimpleDataTypeBool = "bool";
        

        protected string uri;
        protected string type;
        //// creted from shema
        protected ProfileElementTypes typeAsEnumValue = ProfileElementTypes.Unknown;
        
        protected string label;
        protected string comment;
        protected List<ProfileElementStereotype> stereotypes;

        //// class category specifics
        protected List<ProfileElement> membersOfClassCategory;

        //// class specifics
        protected string subClassOf;
        protected string belongsToCategory;
        //// creted from shema        
        protected ProfileElement subClassOfAsObject;
        protected List<ProfileElement> mySubclasses;
        protected ProfileElement belongsToCategoryAsObject;
        protected List<ProfileElement> myProperties;
        protected bool isEnumeration = false;
        protected List<ProfileElement> myEnumerationMembers;  //// if class is enumeration        
        protected bool isExpectedAsLocal = false; //// if the instance of class is expected to be defined inside other instance

        //// attribute specifics
        protected string domain;    //// class to which attribute belongs
        protected string dataType;  //// attribute type
        protected string range;     //// moze biti tip atributa ili ??
        protected string multiplicityAsString = MultiplicityZeroOrOneString;
        //// creted from shema
        protected ProfileElement domainAsObject;
        protected bool isDataTypeSimple = false; ////da li je tip atributa prost(float, bool...) ili je neki od profilskih tipova
        protected Type dataTypeAsSimple;
        protected ProfileElement dataTypeAsComplexObject;
        protected ProfileElement rangeAsObject;
        protected ProfileElementMultiplicity multiplicity = ProfileElementMultiplicity.ZeroOrOne;
        protected bool isExpectedToContainLocalClass = false; //// if this property expected to contain inner instance of some class
       
        //// asociation-attribute specifics
        protected string inverseRoleName; //// name of other side of asociation
        protected bool isAggregate;
        //// creted from shema        
        protected ProfileElement inverseRoleAsObject; //// other side of asociation

        //// enum specifics
        protected ProfileElement enumerationObject; //// if this ProfileElement is a value in some Enumeration, this is that Enumeration


        public ProfileElement()
        {
        }

        public ProfileElement(string uri)
        {
            this.uri = uri;
        }


        /// <summary>
        /// Gets and sets the full URI of profile element.
        /// </summary>
        public string URI {
            get
            {
                return uri;
            }
            set
            {
                uri = value;
            }
        }

        /// <summary>
        /// Gets the part of URI after the '#' simbol
        /// </summary>
        public string UniqueName
        {
            get
            {
                string uniqueName = StringManipulationManager.ExtractShortestName(uri, Separator);                
                return uniqueName;
            }            
        }

        /// <summary>
        /// Gets the most simple fullName ( URI.sustring('#').substring('.') )
        /// </summary>
        public string Name
        {
            get
            {
                string name = StringManipulationManager.ExtractShortestName(uri, Separator);
                name = StringManipulationManager.ExtractShortestName(name, StringManipulationManager.SeparatorDot);
                return name;
            }
        }

        /// <summary>
        /// Gets and sets the type of given profile element in string format.
        /// </summary>
        public string Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;

                string temp = StringManipulationManager.ExtractShortestName(type, Separator);
                switch (temp.ToLower())
                {
                    case TypeClassCategoryString:
                        {
                            typeAsEnumValue = ProfileElementTypes.ClassCategory;
                            break;
                        }
                    case TypeClassString:
                        {
                            typeAsEnumValue = ProfileElementTypes.Class;
                            break;
                        }
                    case TypePropertyString:
                        {
                            typeAsEnumValue = ProfileElementTypes.Property;
                            break;
                        }
                    case TypeStereotypeString:
                        {
                            typeAsEnumValue = ProfileElementTypes.Stereotype;
                            break;
                        }
                    default:
                        {
                            typeAsEnumValue = ProfileElementTypes.Unknown;
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Gets and sets the type of given profile element in ProfileElementTypes enumeration format
        /// </summary>
        public ProfileElementTypes TypeAsEnumValue 
        {
            get
            {
                return typeAsEnumValue;
            }
            set
            {
                typeAsEnumValue = value;               
            }
        }

        /// <summary>
        /// Gets and sets the label of profile element.
        /// </summary>
        public string Label 
        {
            get
            {
                return label;
            }
            set
            {
                label = value;
            }
        }

        /// <summary>
        /// gets and sets the comment atached to the profile element.
        /// </summary>
        public string Comment 
        {
            get
            {
                return comment;
            }
            set
            {
                comment = value;
            }
        }

        /// <summary>
        /// Gets stereotypes of profile element (element can have more then one stereotype)
        /// </summary>
        public List<ProfileElementStereotype> Stereotypes 
        {
            get
            {
                return stereotypes;
            }
        }


        #region Class category specifics
        /// <summary>
        /// Class category Specific property.
        /// <para>Gets and sets the list of all members of this class category.</para>
        /// </summary>
        [BrowsableAttribute(false)]
        public List<ProfileElement> MembersOfClassCategory
        {
            get
            {
                return membersOfClassCategory;
            }
            set 
            {
                membersOfClassCategory = value;
            }
        }

        /// <summary>
        /// Class category specific property.
        /// <para>Gets the number of all members of this class category.</para>
        /// <para>Memeber of class category can be another class category or class.</para>
        /// </summary>
        public int CountMembersOfClassCategory
        {
            get
            {
                int count = 0;
                if (membersOfClassCategory != null)
                {
                    count = membersOfClassCategory.Count;
                }
                return count;
            }
        }
        #endregion Class category specifics

        #region Class specifics
        /// <summary>
        /// Class Specific property.
        /// <para>Gets and sets the URI string of super class(base class).</para>
        /// </summary>
        public string SubClassOf 
        {
            get
            {
                return subClassOf;
            }
            set
            {
                subClassOf = value;
            }
        }

        /// <summary>
        /// Class Specific property.
        /// <para>Gets and sets the URI string of parent package i.e. parent class category.</para>
        /// </summary>
        public string BelongsToCategory 
        {
            get
            {
                return belongsToCategory;
            }
            set
            {
                belongsToCategory = value;
            }
        }

    //// Creted from shema
        /// <summary>
        /// Class Specific property.
        /// <para>Gets and sets the super(base) class ProfileElement.</para>
        /// </summary>
        public ProfileElement SubClassOfAsObject 
        {
            get
            {
                return subClassOfAsObject;
            }
            set
            {
                subClassOfAsObject = value;
            }
        }

        /// <summary>
        /// Class Specific property.
        /// <para>Gets the direct descendants of this ProfileElement if it is of type ProfileElementTypes.Class.</para>
        /// <remarks>To addDocumentToList descendant element use the <seealso cref="M:AddToMySubclasses"/> method</remarks>
        /// </summary>
        public List<ProfileElement> MySubclasses
        {
            get 
            {
                return mySubclasses;
            }            
        }

        /// <summary>
        /// Class Specific property.
        /// <para>Gets all of the descendant elements of this ProfileElement if 
        /// it is of type ProfileElementTypes.Class.</para>
        /// <para>Value is not a tree structure, but a list of all descendant classes, not just direct descendants.</para>
        /// </summary>
        public List<ProfileElement> MySubclassTree
        {
            get
            {               
                return ExtractAllDescendantSubclassesOf(this);
            }
        }

        /// <summary>
        /// Class Specific property.
        /// <para>Gets and sets the parent package i.e. parent class category. </para>
        /// </summary>
        public ProfileElement BelongsToCategoryAsObject 
        {
            get 
            {
                return belongsToCategoryAsObject;
            }
            set 
            {
                belongsToCategoryAsObject = value;
            }
        }

        /// <summary>
        /// Class Specific property.
        /// <para>Gets the list of profile elements which are the properties of given profile class.</para>
        /// </summary>
        public List<ProfileElement> MyProperties {
            get
            {
                return myProperties;
            }
        }

        /// <summary>
        /// Class Specific property.
        /// <para>Gets the list of profile elements which are the properties of given profile class,</para>
        /// <para>as well as all the inhereted properties.</para>  
        /// </summary>
        public List<ProfileElement> MyAndInheritedProperties
        {
            get
            {
                List<ProfileElement> allProperties = myProperties;
                
                ProfileElement superClass = SubClassOfAsObject;
                while (superClass != null)
                {
                    if (superClass.MyProperties != null)
                    {
                        foreach (ProfileElement propertie in superClass.MyProperties)
                        {
                            if (allProperties == null)
                            {
                                allProperties = new List<ProfileElement>();
                            }
                            if (!allProperties.Contains(propertie))
                            {
                                allProperties.Add(propertie);
                            }
                        }
                    }
                    superClass = superClass.SubClassOfAsObject;
                }
                
                return allProperties;
            }
        }

        /// <summary>
        /// Class Specific property.
        /// <para>Gets the indicator whether or not given profile element is enumeration class.</para>
        /// </summary>
        public bool IsEnumeration
        {
            get
            {
                return isEnumeration;
            }
        }

        /// <summary>
        /// Class Specific property.
        /// <para>If profile element is enumeration class, property gets all enumeration elements.</para>
        /// </summary>
        public List<ProfileElement> MyEnumerationMembers
        {
            get
            {
                return myEnumerationMembers;
            }
        }

        /// <summary>
        /// Class Specific property.
        /// <para>If the instance of class is expected to be defined inside instance of another class (as local).</para>
        /// </summary>
        public bool IsExpectedAsLocal
        {
            get
            {
                return isExpectedAsLocal;
            }
            set
            {
                isExpectedAsLocal = value;
            }
        }
        #endregion Class specifics

        #region Property specifics
        /// <summary>
        /// Property Specific property.
        /// <para>Gets and sets the URI of profile element which is the parent class of this profile property.</para>
        /// </summary>
        public string Domain 
        {
            get
            {
                return domain;
            }
            set
            {
                domain = value;
            }
        }

        /// <summary>
        /// Property Specific property.
        /// <para>Gets and sets the string representation of data type for this profile property.</para>
        /// </summary>
        public string DataType  
        {
            get
            {
                return dataType;
            }
            set
            {
                dataType = value;

                if (dataType != null) 
                {
                    isDataTypeSimple = true;
                    string shortDT = StringManipulationManager.ExtractShortestName(dataType, Separator);
                    switch (shortDT.ToLower())
                    {
                        case SimpleDataTypeInteger:
                            {
                                dataTypeAsSimple = System.Type.GetType("System.Int32");
                                break;
                            }
                        case SimpleDataTypeInt:
                            {
                                dataTypeAsSimple = System.Type.GetType("System.Int32");
                                break;
                            }
                        case SimpleDataTypeFloat:
                            {
                                dataTypeAsSimple = System.Type.GetType("System.Single");
                                break;
                            }
                        case SimpleDataTypeString:
                            {
                                dataTypeAsSimple = System.Type.GetType("System.String");
                                break;
                            }
                        case SimpleDataTypeBoolean:
                            {
                                dataTypeAsSimple = System.Type.GetType("System.Boolean");
                                break;
                            }
                        case SimpleDataTypeBool:
                            {
                                dataTypeAsSimple = System.Type.GetType("System.Boolean");
                                break;
                            }
                        case SimpleDataTypeDateTime:
                            {
                                dataTypeAsSimple = System.Type.GetType("System.DateTime");
                                break;
                            }
                        default:
                            {
                                isDataTypeSimple = false;
                                break;
                            }
                    }
                }
            }
        }

        /// <summary>
        /// Property Specific property.
        /// <para>Gets and sets the string representation of range for this profile property.</para>
        /// </summary>
        public string Range
        {
            get
            {
                return range;
            }
            set
            {
                range = value;
            }
        }

    //// creted from shema
        /// <summary>
        /// Property Specific property.
        /// <para>Gets and sets the ProfileElement which is cosidered for parent class of this profile property.</para>
        /// <para>Given ProfileElement should be of type ProfileElementTypes.Class.</para>
        /// </summary>
        public ProfileElement DomainAsObject
        {
            get
            {                
                return domainAsObject;
            }
            set
            {
                domainAsObject = value;
            }
        }

        /// <summary>
        /// Property Specific property.
        /// <para>Gets the indication whether or not the data type(or range) of this profile property element is simple.</para>
        /// </summary>
        public bool IsPropertyDataTypeSimple
        {
            get
            {
                return isDataTypeSimple;
            }
        }

        /// <summary>
        /// Property Specific property.
        /// <para>Gets and sets the CIMType which is the data type of this profile property.</para>        
        /// <para>This can be null if IsPropertyDataTypeSimple has <c>false</c> value.</para>
        /// </summary>
        public Type DataTypeAsSimple
        {
            get
            {
                return dataTypeAsSimple;
            }
            set
            {
                dataTypeAsSimple = value;
            }
        }

        /// <summary>
        /// Property Specific property.
        /// <para>Gets and sets the ProfileElement which is cosidered for data type of this profile property.</para>
        /// <para>Given ProfileElement should be of type ProfileElementTypes.Class.</para>
        /// <para>This can be null if IsPropertyDataTypeSimple has <c>true</c> value.</para>
        /// </summary>
        public ProfileElement DataTypeAsComplexObject
        {
            get
            {
                return dataTypeAsComplexObject;
            }
            set
            {
                dataTypeAsComplexObject = value;
            }
        }

        /// <summary>
        /// Property Specific property.
        /// <para>Gets and sets the ProfileElement which is cosidered for data range of this profile property.</para>
        /// <para>Given ProfileElement should be of type ProfileElementTypes.Class.</para>
        /// <para>This can be null if IsPropertyDataTypeSimple has <c>true</c> value.</para>
        /// <para>See also: <see cref="P:DataTypeAsComplexObject"/></para>
        /// </summary>
        public ProfileElement RangeAsObject
        {
            get
            {
                return rangeAsObject;
            }
            set
            {
                rangeAsObject = value;
            }
        }

    //// creted from shema
        /// <summary>
        /// Property Specific property.
        /// <para>Gets and sets the multiplicity of this profile element.</para>
        /// </summary>
        public ProfileElementMultiplicity Multiplicity
        {
            get
            {
                return multiplicity;
            }
            set
            {
                multiplicity = value;
            }
        }

        /// <summary>
        /// Property Specific property.
        /// <para>Gets and sets the multiplicity of profile element(property) in string format.</para>
        /// </summary>
        public string MultiplicityAsString
        {
            set
            {
                multiplicityAsString = value;
                switch (value)
                {
                    case MultiplicityZeroOrOneString:
                        {
                            multiplicity = ProfileElementMultiplicity.ZeroOrOne;
                            break;
                        }
                    case MultiplicityExactlyOneString1:
                        {
                            multiplicity = ProfileElementMultiplicity.ExactlyOne;
                            break;
                        }
                    case MultiplicityExactlyOneString2:
                        {
                            multiplicity = ProfileElementMultiplicity.ExactlyOne;
                            break;
                        }
                    case MultiplicityZeroOrMoreString:
                        {
                            multiplicity = ProfileElementMultiplicity.ZeroOrMore;
                            break;
                        }
                    case MultiplicityOneOrMoreString:
                        {
                            multiplicity = ProfileElementMultiplicity.OneOrMore;
                            break;
                        }
                    default:
                        {
                            multiplicity = ProfileElementMultiplicity.ZeroOrOne;
                            break;
                        }
                }
            }
            get
            {
                return multiplicityAsString;
            }
        }

        /// <summary>
        /// Property Specific property.
        /// <para>If this property is expected to contain inner(local) instance(s) of "Range" class.</para>
        /// </summary>    
        public bool IsExpectedToContainLocalClass
        {
            get
            {
                return isExpectedToContainLocalClass;
            }
            set
            {
                isExpectedToContainLocalClass = value;
            }
        }

    //// asociation-attribute specifics
        /// <summary>
        /// Property Specific property.
        /// <para>Gets and sets the URI string of inverse ProfileElement i.e. inverse property element.</para>
        /// </summary>
        public string InverseRoleName
        {
            get
            {
                return inverseRoleName;
            }
            set
            {
                inverseRoleName = value;
            }
        }

        /// <summary>
        /// Property Specific property.
        /// <para>Gets and sets the indication whether or not this profile property is agregation.</para>
        /// </summary>
        public bool IsAggregate
        {
            get
            {
                return isAggregate;
            }
            set
            {
                isAggregate = value;
            }
        }

        /// <summary>
        /// Property Specific property.
        /// <para>Gets and sets the inverse ProfileElement i.e. inverse property element.</para>
        /// <para>This property can be null if the InverseRole isn't defined.</para>
        /// </summary>
        public ProfileElement InverseRoleAsObject // veza sa druge strane 
        {
            get
            {
                return inverseRoleAsObject;
            }
            set
            {
                inverseRoleAsObject = value;
            }
        }
        #endregion Property specifics

        #region Enumeration specifics
        /// <summary>
        /// Enumeration member Specific property.
        /// <para>Gets and sets the ProfileElement which is the parent class of this enumeration member element.</para>        
        /// </summary>
        public ProfileElement EnumerationObject
        {
            get 
            {
                return enumerationObject;
            }
            set
            {
                enumerationObject = value;
            }
        }
        #endregion Enumeration specifics

        /// <summary>
        /// Method creates new instance of ProfileElementStereotype and adds it to ProfileElement's
        /// stereotypes list if it doesn't already exists.
        /// </summary>
        /// <param fullName="fullStereotypeString">the full stereotype fullName</param>
        public void AddStereotype(string fullStereotypeString) 
        {
            if (!string.IsNullOrEmpty(fullStereotypeString))
            {
                ProfileElementStereotype stereotype = Profile.FindOrCreateStereotypeForName(fullStereotypeString);

                if (stereotype != null)
                {
                    if (stereotypes == null)
                    {
                        stereotypes = new List<ProfileElementStereotype>();
                    }

                    if (!stereotypes.Contains(stereotype))
                    {
                        stereotypes.Add(stereotype);
                        stereotypes.Sort(CIMComparer.ProfileElementStereotypeComparer);
                    }

                    if (ProfileElementStereotype.StereotypeEnumeration.Equals(stereotype.Name))
                    {
                        isEnumeration = true;
                    }

                    if (ProfileElementStereotype.StereotypeAggregateOf.Equals(stereotype.Name))
                    {
                        isAggregate = true;
                    }
                }
            }
        }

        /// <summary>
        /// Method checks whether or not given stereotype exist is inside of stereotypes list.
        /// </summary>
        /// <param fullName="stereotype">search for this stereotype</param>
        /// <returns><c>true</c> if stereotype was founded, <c>false</c> otherwise</returns>
        public bool HasStereotype(ProfileElementStereotype stereotype) 
        {
            bool hasStereotype = false;
            if (stereotypes != null)
            {
                hasStereotype = stereotypes.Contains(stereotype);
            }
            return hasStereotype;
        }

        /// <summary>
        /// Method checks whether or not given stereotype exist is inside of stereotypes list.
        /// </summary>
        /// <param fullName="stereotypeName">search for stereotype with this name</param>
        /// <returns><c>true</c> if stereotype was founded, <c>false</c> otherwise</returns>
        public bool HasStereotype(string stereotypeName)
        {
            bool hasStereotype = false;
            if (stereotypes != null)
            {
                foreach (ProfileElementStereotype stereotype in stereotypes)
                {
                    if ((string.Compare(stereotype.Name, stereotypeName) == 0) ||(string.Compare(stereotype.ShortName, stereotypeName) == 0))
                    {
                        hasStereotype = true;
                        break;
                    }
                }
            }
            return hasStereotype;
        }

        /// <summary>
        /// Method checks whether or not this ProfileElement contains 
        /// the "concrete" stereotype inside the stereotypes list.
        /// </summary>
        public bool IsConcrete()
        {
            return this.HasStereotype(Profile.StereotypeList[0]);
        }

        /// <summary>
        /// Method checks whether or not this ProfileElement contains 
        /// the "compaund" stereotype inside the stereotypes list.
        /// </summary>
        public bool IsCompaund()
        {
            return this.HasStereotype(Profile.StereotypeList[1]);
        }

        /// <summary>
        /// Method gets the namespace string for profile element.
        /// <para> Default namespace string is "cim:".</para>
        /// <para>Diferent namespace is returned when a single unstandard stereotype is detected for this profile element.</para>
        /// </summary>
        /// <returns>the string "detected_namespace:" </returns>
        public string GetNamespaceString()
        {
            string namespaceString = CIMConstants.CIMNamespace;
            if (stereotypes != null)
            {
                List<string> notStandard = new List<string>();
                foreach (ProfileElementStereotype stereotype in stereotypes)
                {
                    if ((string.Compare(stereotype.Name, ProfileElementStereotype.StereotypeAggregateOf) == 0) || (string.Compare(stereotype.ShortName, ProfileElementStereotype.StereotypeAggregateOf) == 0)
                        || (string.Compare(stereotype.Name, ProfileElementStereotype.StereotypeAttribute) == 0) || (string.Compare(stereotype.ShortName, ProfileElementStereotype.StereotypeAttribute) == 0)
                        || (string.Compare(stereotype.Name, ProfileElementStereotype.StereotypeByReference) == 0) || (string.Compare(stereotype.ShortName, ProfileElementStereotype.StereotypeByReference) == 0)
                        || (string.Compare(stereotype.Name, ProfileElementStereotype.StereotypeConcrete) == 0) || (string.Compare(stereotype.ShortName, ProfileElementStereotype.StereotypeConcrete) == 0)
                        || (string.Compare(stereotype.Name, ProfileElementStereotype.StereotypeCompound) == 0) || (string.Compare(stereotype.ShortName, ProfileElementStereotype.StereotypeCompound) == 0)
                        || (string.Compare(stereotype.Name, ProfileElementStereotype.StereotypeEnumeration) == 0) || (string.Compare(stereotype.ShortName, ProfileElementStereotype.StereotypeEnumeration) == 0)
                        || (string.Compare(stereotype.Name, ProfileElementStereotype.StereotypeOfAggregate) == 0) || (string.Compare(stereotype.ShortName, ProfileElementStereotype.StereotypeOfAggregate) == 0)
                        || (string.Compare(stereotype.Name, ProfileElementStereotype.StereotypeCompositeOf) == 0) || (string.Compare(stereotype.ShortName, ProfileElementStereotype.StereotypeCompositeOf) == 0))
                    {
                        continue;
                    }
                    else
                    {
                        notStandard.Add(stereotype.ShortName);
                    }
                }
                if (notStandard.Count == 1)
                {
                    namespaceString = notStandard[0];
                }
            }
            namespaceString += StringManipulationManager.SeparatorColon;
            return namespaceString;
        }       

        /// <summary>
        /// Method adds given ProfileElement to the MySubclasses list.
        /// </summary>
        /// <param fullName="subclass"></param>
        public void AddToMySubclasses(ProfileElement subclass)
        {
            if (mySubclasses == null)
            {
                mySubclasses = new List<ProfileElement>();
            }

            if (!mySubclasses.Contains(subclass))
            {
                mySubclasses.Add(subclass);
                mySubclasses.Sort(CIMComparer.ProfileElementComparer);
            }
        }

        /// <summary>
        /// Method adds given ProfileElement to the MembersOfClassCategory list. 
        /// 
        /// </summary>
        /// <param fullName="member"></param>
        public void AddToMembersOfClassCategory(ProfileElement member)
        {
            if (membersOfClassCategory == null)
            {
                membersOfClassCategory = new List<ProfileElement>();
            }

            if (!membersOfClassCategory.Contains(member))
            {
                membersOfClassCategory.Add(member);
                membersOfClassCategory.Sort(CIMComparer.ProfileElementComparer);
            }
        }

        /// <summary>
        /// Method adds given ProfileElement to the MyProperties list. 
        /// </summary>
        /// <param fullName="property"></param>
        public void AddToMyProperties(ProfileElement property) 
        {
            if (myProperties == null)
            {
                myProperties = new List<ProfileElement>();
            }

            if (!myProperties.Contains(property))
            {
                myProperties.Add(property);
                myProperties.Sort(CIMComparer.ProfileElementComparer);
            }
        }

        /// <summary>
        /// Method adds given ProfileElement to the MyEnumerationMembers list. 
        /// </summary>
        /// <param fullName="enumerationMemeber"></param>
        public void AddToMyEnumerationMembers(ProfileElement enumerationMemeber)
        {
            if (enumerationMemeber != null)
            {
                if (!isEnumeration)
                {
                    isEnumeration = true;
                }

                if (myEnumerationMembers == null)
                {
                    myEnumerationMembers = new List<ProfileElement>();
                }

                if (!myEnumerationMembers.Contains(enumerationMemeber))
                {
                    myEnumerationMembers.Add(enumerationMemeber);
                    myEnumerationMembers.Sort(CIMComparer.ProfileElementComparer);
                }
            }
        }

        public override bool Equals(object obj)
        {
            bool eq = false;
            if (obj != null)
            {
                if (obj.GetType().Equals(this.GetType()))
                {
                    ProfileElement pelObj = (ProfileElement)obj;
                    if ((pelObj.URI != null) && (this.URI != null))
                    {
                        eq = this.URI.Equals(pelObj.URI);
                    }
                }
            }       
            return eq;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        

        // Recursive method for getting all of the descendants of given ProfileElement.
        // Method has sense only if given element is of type ProfileElementTypes.Class.
        private List<ProfileElement> ExtractAllDescendantSubclassesOf(ProfileElement element)
        {
            List<ProfileElement> allSubclasses = new List<ProfileElement>();
            if (element != null) 
            {
                if ((element.MySubclasses != null) && (MySubclasses.Count > 0))
                {
                    foreach (ProfileElement subclass in element.MySubclasses)
                    {
                        allSubclasses.AddRange(ExtractAllDescendantSubclassesOf(subclass));
                        allSubclasses.Add(subclass);
                    }
                }
            }
            allSubclasses.Sort(CIMComparer.ProfileElementComparer);
            return allSubclasses;
        }        
    }
}
