
namespace FTN.ESI.SIMES.CIM.Model
{
    public class EnumMember : ProfileElement
    {
        //// enum specifics
        protected ProfileElement enumerationObject; //// if this ProfileElement is a value in some Enumeration, this is that Enumeration

        public EnumMember() : base("From Derived")
        {

        }

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
    }
}
