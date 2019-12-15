using System.ComponentModel;
using CIM.Manager;

namespace CIM.Model
{
    /// <summary>
    /// ProfileElementStereotype is simple class for representing one "stereotype"
    /// definition identified while processing the CIM profile file.
    /// <para>There are several predefined stereotypes (whose names are given by constants in this class),</para>
    /// <para>but by using this class it is possible to support more stereotypes.</para>
    /// <para>See also: <seealso cref="P:Profile.StereotypeList"/></para>
    /// <para>@author: Stanislava Selena</para>
    /// </summary>
    public class ProfileElementStereotype
    {
        /// <summary> "concrete" stereotype </summary>
        public const string StereotypeConcrete = "concrete";
        /// <summary> "compound" stereotype </summary>
        public const string StereotypeCompound = "compound";        
        /// <summary> "enumeration" stereotype </summary>
        public const string StereotypeEnumeration = "enumeration";
        /// <summary> "attribute" stereotype </summary>
        public const string StereotypeAttribute = "attribute";
        /// <summary> "byreference" stereotype </summary>
        public const string StereotypeByReference = "byreference";
        /// <summary> "ofAggregate" stereotype </summary>
        public const string StereotypeOfAggregate = "ofAggregate";
        /// <summary> "aggregateOf" stereotype </summary>
        public const string StereotypeAggregateOf = "aggregateOf";
        /// <summary> "compositeOf" stereotype </summary>
        public const string StereotypeCompositeOf = "compositeOf";

        private int code;
        private string name;


        public ProfileElementStereotype(int code, string name)
        {
            this.code = code;
            this.name = name;
        }

        /// <summary>
        /// Gets and sets the integer code of this ProfileElementStereotype object.
        /// </summary>
        [BrowsableAttribute(false)]
        public int Code
        {
            get 
            {
                return code;
            }
            set
            {
                code = value;
            }
        }

        /// <summary>
        /// Gets and sets the fullName of this ProfileElementStereotype object.
        /// </summary>
        [DescriptionAttribute("Stereotype name."),
         ReadOnlyAttribute(true)]
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        /// <summary>
        /// Gets the simple fullName of stereotype extracted form Name property (last sustring after "#").
        /// </summary>
        [DescriptionAttribute("Short representation of stereotype name."), 
         ReadOnlyAttribute(true),
         DisplayNameAttribute("Simple Name")]
        public string ShortName
        {
            get
            {
                return StringManipulationManager.ExtractShortestName(name, StringManipulationManager.SeparatorSharp);
            }            
        }

        public override bool Equals(object obj)
        {
            bool equal = false;
            if ((obj != null) && (obj is ProfileElementStereotype) && !string.IsNullOrEmpty(this.name))
            {
                equal = string.Equals(((ProfileElementStereotype)obj).name, this.name);
            }                
            return equal;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            string toString = string.Empty;
            if (name != null)
            {
                toString = name;
            }
            return toString;
        }
    }
}
