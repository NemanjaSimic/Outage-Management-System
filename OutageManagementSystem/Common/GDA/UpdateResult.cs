using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System;

namespace Outage.Common.GDA
{
	[Obsolete("Use Common.NmsContracts alternative")]
	public enum ResultType : byte
	{
		Succeeded = 0,	
		Failed = 1	
	}

	[DataContract]
	[Obsolete("Use Common.NmsContracts alternative")]
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
		[Obsolete("Use Common.NmsContracts alternative")]
		public Dictionary<long, long> GlobalIdPairs
		{
			get { return globalIdPairs; }
			set { globalIdPairs = value; }
		}

		[DataMember]
		[Obsolete("Use Common.NmsContracts alternative")]
		public string Message
		{
			get { return message; }
			set { message = value; }
		}

		[DataMember]
		[Obsolete("Use Common.NmsContracts alternative")]
		public ResultType Result
		{
			get { return result; }
			set { result = value; }
		}

		[Obsolete("Use Common.NmsContracts alternative")]
		public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("Update result: {0}\n", result);
            sb.AppendFormat("Message: {0}\n", message);
            sb.AppendLine("GlobalId pairs:");

            foreach (KeyValuePair<long, long> kvp in globalIdPairs)
            {
                sb.AppendFormat("Client globalId: 0x{0:X16}\t - Server globalId: 0x{1:X16}\n", kvp.Key, kvp.Value);
            }

            return sb.ToString();
        }
	}
}
