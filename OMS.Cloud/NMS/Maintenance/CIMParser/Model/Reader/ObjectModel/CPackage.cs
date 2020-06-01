using System.Collections.Generic;

namespace CIM.Model
{
    internal class CPackage
    {
        public string name;
        public List<CClass> classes = new List<CClass>();
    }
}