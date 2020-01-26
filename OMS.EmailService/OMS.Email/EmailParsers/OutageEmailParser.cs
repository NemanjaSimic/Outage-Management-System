using OMS.Email.Interfaces;
using OMS.Email.Models;
using System;

namespace OMS.Email.EmailParsers
{
    public class OutageEmailParser : IEmailParser
    {
        public OutageTracingModel Parse(OutageMailMessage message)
        {
            string searchQuery = "gid:";
            int gidIndex = message.Body.ToLower().IndexOf(searchQuery);

            if (gidIndex < 0)
                return BuildInvalidModel();

            long gid = 0;
            int gidLength = 7;
            try
            {
                // we should consider everything 
                string gidText = message.Body.Substring(gidIndex + searchQuery.Length, gidLength + 1);
                gid = Int64.Parse(gidText);
            }
            catch (Exception)
            {
                return BuildInvalidModel();
            }

            return BuildValidModel(gid);
        }

        private OutageTracingModel BuildInvalidModel() => new OutageTracingModel { Gid = 0, IsValidReport = false };
        private OutageTracingModel BuildValidModel(long gid) => new OutageTracingModel { Gid = gid, IsValidReport = true };
    }
}
