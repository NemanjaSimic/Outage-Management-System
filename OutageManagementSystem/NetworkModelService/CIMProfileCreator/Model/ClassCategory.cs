using System.Collections.Generic;
using FTN.ESI.SIMES.CIM.Model.Tools;

namespace FTN.ESI.SIMES.CIM.Model
{
    public class ClassCategory : ProfileElement
    {
        protected string belongsToCategory;
        protected List<ProfileElement> membersOfClassCategory;
        protected ClassCategory belongsToCategoryAsObject;

        public ClassCategory() : base("From Derived")
        {

        }


        #region Class category specifics
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
        /// Class category Specific property.
        /// <para>Gets and sets the list of all members of this class category.</para>
        /// </summary>
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

        public ClassCategory BelongsToCategoryAsObject
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
        #endregion Class category specifics


        /// <summary>
        /// Method adds given ProfileElement to the MembersOfClassCategory list.
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
    }
}
