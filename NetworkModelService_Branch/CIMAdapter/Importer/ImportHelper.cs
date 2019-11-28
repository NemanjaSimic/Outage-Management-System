using Outage.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.DataImporter.CIMAdapter.Importer
{
    public class ImportHelper
    {
        private Dictionary<DMSType, int> typeCounter = new Dictionary<DMSType, int>();
        private Dictionary<string, long> rdfIDtoGIDMapping = new Dictionary<string, long>();

        /// <summary>
		/// Method calculates the next negative index for given DMSType.
		/// </summary>
		/// <param name="dmsType"></param>
		/// <returns></returns>
        public int CheckOutIndexForDMSType(DMSType dmsType)
        {
            int counter = -1;

            lock (typeCounter)
            {
                int temp;
                if (typeCounter.TryGetValue(dmsType, out temp))
                {
                    counter = temp - 1;
                    typeCounter[dmsType] = counter;
                }
                else
                {
                    typeCounter.Add(dmsType, counter);
                }
            }

            return counter;
        }

        /// <summary>
		/// Method inserts or updates the id mapping: CIM rdfID to NMS gid.
		/// </summary>
		/// <param name="rdfID"></param>
		/// <param name="gid"></param>
        public void DefineIDMapping(string rdfID, long gid)
        {
            if (rdfIDtoGIDMapping.ContainsKey(rdfID))
            {
                rdfIDtoGIDMapping[rdfID] = gid;
            }
            else
            {
                rdfIDtoGIDMapping.Add(rdfID, gid);
            }
        }

        /// <summary>
		/// Method returns the value of gid that is mapped to given rdfID. If there is no mapping defined, method returns -1.
		/// </summary>
		/// <param name="rdfID">CIM rdfID value</param>
		/// <returns>gid or -1 (if mapping not defined)</returns>
        public long GetMappedGID(string rdfID)
        {
            long gid = -1;
            if (!rdfIDtoGIDMapping.TryGetValue(rdfID, out gid))
            {
                gid = -1;
            }
            return gid;
        }
    }
}
