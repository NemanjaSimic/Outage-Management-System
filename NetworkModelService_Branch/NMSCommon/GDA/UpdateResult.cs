using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace FTN.Common
{	
	public enum ResultType : byte
	{
		Succeeded = 0,	
		Failed = 1	
	}

	[DataContract]
	public class UpdateResult
	{
		private Dictionary<long, long> globalIdPairs;
		private string message;
		private ResultType result;		

		public UpdateResult()			
		{
			globalIdPairs = new Dictionary<long, long>();
			message = string.Empty;
			result = ResultType.Succeeded;			
		}

		[DataMember]
		public Dictionary<long, long> GlobalIdPairs
		{
			get { return globalIdPairs; }
			set { globalIdPairs = value; }
		}

		[DataMember]
		public string Message
		{
			get { return message; }
			set { message = value; }
		}

		[DataMember]
		public ResultType Result
		{
			get { return result; }
			set { result = value; }
		}

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("Update result: {0}\n", result);
            sb.AppendFormat("Message: {0}\n", message);
            sb.AppendLine("GlobalId pairs:");

            foreach (KeyValuePair<long, long> kvp in globalIdPairs)
            {
                sb.AppendFormat("Client globalId: 0x{0:x16}\t - Server globalId: 0x{1:x16}\n", kvp.Key, kvp.Value);
            }

            return sb.ToString();
        }
	}
}
