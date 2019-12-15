using System.ComponentModel;
using System.Configuration;

namespace OMS.Web.Common
{
    public static class AppSettings
    {
        public static T Get<T>(string key)
        {
            string setting = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrWhiteSpace(setting))
            {
                throw new ConfigurationErrorsException($"Key '{key}' not found in the configuration file!");
            }

            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)converter.ConvertFromInvariantString(setting);
        }

        public static T Get<T>(string key, T defaultValue)
        {
            string setting = ConfigurationManager.AppSettings[key];
            return string.IsNullOrWhiteSpace(setting) ? defaultValue : (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(setting);
        }
    }
}
