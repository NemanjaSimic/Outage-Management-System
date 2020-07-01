using OMS.Common.Cloud.Logger;
using System;
using System.Reflection;

namespace Outage.DataImporter.CIMAdapter.Manager
{
    public enum SupportedProfiles : byte
    {
        Outage,
    };

    /// <summary>
	/// ProfileManager
	/// </summary>
	public class ProfileManager
    {
        public const string Namespace = "Outage";
        public const string ProductName = "NetworkModelService";

        #region Private Properties
        private static ICloudLogger logger;

        protected static ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
        #endregion Private Properties

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
            return string.Format("{0}CIMProfile_{1}.dll", profile.ToString(), ProductName);
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
                Logger.LogError($"Error during Assembly load. Profile: {profile} ; Message: {e.Message}", e);
                
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
                Logger.LogError($"Error during Assembly load. Path: {path} ; Message: {e.Message}");
                return false;
            }
            return true;
        }
    }
}
