using System.Collections.Generic;

namespace FTN.ESI.SIMES.CIM.Model.Tools
{
    /// <summary>
    /// ModelElementComparer class is impelemntation of IComparer interface.
    /// <para>It is used for comparing two objects, and is designed for ProfileElement and ProfileElementStereotype objects.</para>
    /// <para>This class can be used for sorting lists of objects.</para>
    /// </summary>
    /// <typeparam name="T">one of the: ProfileElement or ProfileElementStereotype class</typeparam>
    public class ModelElementComparer<T> : IComparer<T> 
    {        
        /// <summary>
        /// Method implements method for comparing two objects.
        /// <para>Method is applicable only if both of given objects are instances of the same class.</para>
        /// <para>Method designed is for ProfileElement and ProfileElementStereotype objects.</para>
        /// <para>This is impelemtation of IComparer interface. </para>
        /// </summary>
        /// <param name="modelObject1">object for comparing</param>
        /// <param name="modelObject2">object for comparing</param>
        /// <returns></returns>
        public int Compare(T modelObject1, T modelObject2)
        {
            int i = 0;
            if (modelObject1 != null)
            {
                i = 1;
                if (modelObject2 != null)
                {
                    if ((modelObject1 is ProfileElement) && (modelObject2 is ProfileElement))
                    {
                        i = string.Compare((modelObject1 as ProfileElement).UniqueName, (modelObject2 as ProfileElement).UniqueName);
                    }
                    else if ((modelObject1 is ProfileElementStereotype) && (modelObject2 is ProfileElementStereotype))
                    {
                        i = string.Compare((modelObject1 as ProfileElementStereotype).ShortName, (modelObject2 as ProfileElementStereotype).ShortName);
                    } 

                }
            }
            return i;
        }

    } 
}
