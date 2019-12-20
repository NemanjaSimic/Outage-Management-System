using System.Collections;
using System.Collections.Generic;

namespace CIM.Model
{
    internal class CObjectModel
    {
        public List<CPackage> packages = new List<CPackage>();
        public Hashtable classes = new Hashtable();
        public Hashtable links = new Hashtable();
        public List<CAssociation> m_LeafAssoc = new List<CAssociation>();
    }
}