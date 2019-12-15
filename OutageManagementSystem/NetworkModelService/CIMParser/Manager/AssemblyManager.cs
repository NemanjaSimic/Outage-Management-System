using System.Reflection;

namespace CIM.Manager
{
    public class AssemblyManager
    {
        public static bool LoadAssembly(string path, out Assembly assembly)
        {
            try
            {
                assembly = Assembly.LoadFrom(path);
            }
            catch 
            {
                assembly = null;
                return false;
            }
            return true;
        }
    }
}
