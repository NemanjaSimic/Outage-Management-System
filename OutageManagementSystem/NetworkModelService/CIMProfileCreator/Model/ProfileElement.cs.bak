
using System.Collections.Generic;
using FTN.ESI.SIMES.CIM.Manager;
using FTN.ESI.SIMES.CIM.Model.Tools;
namespace FTN.ESI.SIMES.CIM.Model
{
    /// <summary>
    /// Possible types of profile elements
    /// </summary>
    public enum ProfileElementTypes
    {
        Unknown = 0, ClassCategory, Class, Property, EnumerationElement, Stereotype
    };

    /// <summary>
    /// ProfileElement class represents one element founded during processing of profile's source file.
    /// <para>See also: <seealso cref="T:Profile"/></para>
    /// </summary>
    public abstract class ProfileElement
    {
        /// <summary> string with value "classcategory" </summary>
        public const string TypeClassCategoryString = "classcategory";
        /// <summary> string with value "class" </summary>
        public const string TypeClassString = "class";
        /// <summary> string with value "property" </summary>
        public const string TypePropertyString = "property";
        /// <summary> string with value "stereotype" </summary>
        public const string TypeStereotypeString = "stereotype";

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
        
        protected string label;
        protected string comment;

        protected string multiplicityAsString = "M:0..1";
        protected List<ProfileElementStereotype> stereotypes;
        protected bool isAggregate;
        protected bool isEnumeration = false;


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
        public string URI
        {
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
        /// Gets and sets the multiplicityAsString of profile element.
        /// </summary>
        public string MultiplicityAsString
        {
            get
            {
                return multiplicityAsString;
            }
            set
            {
                multiplicityAsString = value;
            }
        }

        /// <summary>
        /// Gets the part of URI after the '#' simbol
        /// </summary>
        public string UniqueName
        {
            get
            {
                string uniqueName = StringManipulationManager.ExtractShortestName(uri, StringManipulationManager.SeparatorSharp);                
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
                string name = StringManipulationManager.ExtractShortestName(uri, StringManipulationManager.SeparatorSharp);
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
		/// Class Specific property.
		/// <para>Gets the indicator whether or not given profile element is enumeration class.</para>
		/// </summary>
        public bool IsEnumeration
        {
            get
            {
                return isEnumeration;
            }
            set
            {
                isEnumeration = value;
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

        #region stereotype
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
                    if ((string.Compare(stereotype.Name, stereotypeName) == 0) || (string.Compare(stereotype.ShortName, stereotypeName) == 0))
                    {
                        hasStereotype = true;
                        break;
                    }
                }
            }
            return hasStereotype;
        }

        public List<ProfileElementStereotype> GetUndefinedStereotypes()
        {
            List<ProfileElementStereotype> undefinedStereotypes = new List<ProfileElementStereotype>();
            if (stereotypes == null || stereotypes.Count <= 0)
            {
                return null;
            }
            foreach (ProfileElementStereotype stereotype in stereotypes)
            {
                if (!stereotype.Name.Equals(ProfileElementStereotype.StereotypeConcrete) && !stereotype.Name.Equals(ProfileElementStereotype.StereotypeCompound) && !stereotype.Name.Equals(ProfileElementStereotype.StereotypeEnumeration) && !stereotype.Name.Equals(ProfileElementStereotype.StereotypeAttribute) && !stereotype.Name.Equals(ProfileElementStereotype.StereotypeByReference) && !stereotype.Name.Equals(ProfileElementStereotype.StereotypeOfAggregate) && !stereotype.Name.Equals(ProfileElementStereotype.StereotypeAggregateOf) && !stereotype.Name.Equals(ProfileElementStereotype.StereotypeCompositeOf))
                {
                    undefinedStereotypes.Add(stereotype);

                }
            }
            return undefinedStereotypes;
        }
        #endregion
    }
}
