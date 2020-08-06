using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace OMS.Common.NmsContracts.GDA
{
	public enum ResultType : byte
	{
		Succeeded = 1,	
		Failed = 2	
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
            sb.AppendLine("GlobalId pairs:");

            foreach (KeyValuePair<long, long> kvp in globalIdPairs)
            {
                sb.AppendFormat($"Client globalId: 0x{kvp.Key:X16}\t - Server globalId: 0x{kvp.Value:X16}\n");
            }

            sb.AppendFormat("Message: {0}\n", message);

            return sb.ToString();
        }
	}
}
