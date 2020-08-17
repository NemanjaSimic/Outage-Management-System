using OMS.EmailImplementation.Interfaces;
using OMS.EmailImplementation.Models;
using OMS.Common.Cloud.Logger;
using System;
using System.Globalization;

namespace OMS.EmailImplementation.EmailParsers
{
    public class OutageEmailParser : IEmailParser
    {
        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
        public OutageTracingModel Parse(OutageMailMessage message)
        {
            string searchQuery = "gid";
            int gidIndex = message.Body.ToLower().IndexOf(searchQuery);

            if (gidIndex < 0)
            {
               Logger.LogWarning("Gid Index is less than 0. Invalid model will be build.");
               return BuildInvalidModel();
            }

            long gid = 0;

            try
            {
                // we should consider everything 
                string gidText = message.Body.Split('[')[1].Split(']')[0];

                if (gidText.ToLower().Contains("0x"))
                {
                    gid = long.Parse(gidText.ToLower().Split('x')[1], NumberStyles.HexNumber);
                }
                else
                {
                    gid = long.Parse(gidText);
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Exception on parsing outage email, invalid model will be build. Message: {e.Message}");
                return BuildInvalidModel();
            }

            return BuildValidModel(gid);
        }

        private OutageTracingModel BuildInvalidModel() => new OutageTracingModel { Gid = 0, IsValidReport = false };
        private OutageTracingModel BuildValidModel(long gid) => new OutageTracingModel { Gid = gid, IsValidReport = true };
    }
}
