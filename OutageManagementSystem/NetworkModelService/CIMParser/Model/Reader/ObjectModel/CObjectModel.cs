using System.Collections.Generic;
using System.Collections;

namespace CIM.Model
{
    class CObjectModel
    {
        public List<CPackage> packages = new List<CPackage>();
        public Hashtable classes = new Hashtable();
        public Hashtable links = new Hashtable();
        public List<CAssociation> m_LeafAssoc = new List<CAssociation>();
    }
}
