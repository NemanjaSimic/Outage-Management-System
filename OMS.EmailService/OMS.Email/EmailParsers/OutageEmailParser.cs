namespace OMS.Email.EmailParsers
{
    using OMS.Email.Interfaces;
    using OMS.Email.Models;
    using System;
    using System.Globalization;

    public class OutageEmailParser : IEmailParser
    {
        public OutageTracingModel Parse(OutageMailMessage message)
        {
            string searchQuery = "gid";
            int gidIndex = message.Body.ToLower().IndexOf(searchQuery);

            if (gidIndex < 0)
                return BuildInvalidModel();

            long gid = 0;

            try
            {
                // we should consider everything 
                string gidText = message.Body.Split('[')[1].Split(']')[0];

                if(gidText.ToLower().Contains("0x"))
                {
                    gid = long.Parse(gidText.ToLower().Split('x')[1], NumberStyles.HexNumber);
                }
                else
                {
                    gid = long.Parse(gidText);
                }
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
