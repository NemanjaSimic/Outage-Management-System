using System.Collections.Generic;

namespace CIM.Model
{
    enum ClassContainment
    { 
        NotContained,
        ContainedInternal,
        ContainedExternal
    };
    
    class CClass
    {
        public string name;
        public string stereotype;
        public string parentClassName;
        public List<CClass> childClasses = new List<CClass>();
        public List<CAssociation> associations = new List<CAssociation>();
        public List<CAttribute> attributes = new List<CAttribute>();

        public CAttribute GetGidAttribute()
        {
            for (int i = 0; i < attributes.Count; i++)
            {
                if (attributes[i].name == "GID")
                    return attributes[i];
            }
            return null;
        }
    }
}
