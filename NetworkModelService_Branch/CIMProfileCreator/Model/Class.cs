using System.Collections.Generic;
using FTN.ESI.SIMES.CIM.Model.Tools;

namespace FTN.ESI.SIMES.CIM.Model
{
    public class Class : ProfileElement
    {
        protected string belongsToCategory;
        protected ProfileElement belongsToCategoryAsObject;
        protected string subClassOf;

        protected List<ProfileElement> myProperties;

        protected List<ProfileElement> mySubclasses;
        protected ProfileElement subClassOfAsObject;

        protected List<ProfileElement> myEnumerationMembers;  //// if class is enumeration        


        public Class() : base("From Derived")
        {

        }

        
        #region Class specifics
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
        public List<ProfileElement> MyProperties
        {
            get
            {
                return myProperties;
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
        #endregion Class specifics
    }
}
