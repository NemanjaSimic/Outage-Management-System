using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Outage.CIMAdapter.Manager
{
    public enum SupportedProfiles : byte
    {
        OutageProfile,
    };

    /// <summary>
	/// ProfileManager
	/// </summary>
	public static class ProfileManager
    {
        public const string Namespace = "Outage";

        /// <summary>
        /// Method returns the name of CIM profile based on the defined enumeration.
        /// </summary>
        /// <param name="profile">supported CIM profile</param>
        /// <returns>name of profile + "CIMProfile_Labs"</returns>
        public static string GetProfileName(SupportedProfiles profile)
        {
            return string.Format("{0}CIMProfile_Labs", profile.ToString());
        }

        /// <summary>
        /// Method returns the name of the CIM profile DLL based on the defined enumeration.
        /// </summary>
        /// <param name="profile">supported CIM profile</param>
        /// <returns>name of profile + "CIMProfile_Labs.DLL"</returns>
        public static string GetProfileDLLName(SupportedProfiles profile)
        {
            return string.Format("{0}CIMProfile_Labs.dll", profile.ToString());
        }

        public static bool LoadAssembly(SupportedProfiles profile, out Assembly assembly)
        {
            try
            {
                string assemblyFile = string.Format(".\\{0}", ProfileManager.GetProfileDLLName(profile));
                assembly = Assembly.LoadFrom(assemblyFile);
            }
            catch (Exception e)
            {
                assembly = null;
                //LogManager.Log(string.Format("Error during Assembly load. Profile: {0} ; Message: {1}", profile, e.Message), LogLevel.Error);
                return false;
            }
            return true;
        }

        public static bool LoadAssembly(string path, out Assembly assembly)
        {
            try
            {
                assembly = Assembly.LoadFrom(path);
            }
            catch (Exception e)
            {
                assembly = null;
                //LogManager.Log(string.Format("Error during Assembly load. Path: {0} ; Message: {1}", path, e.Message), LogLevel.Error);
                return false;
            }
            return true;
        }
    }
}
