using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace FTN.Common
{
	
	[DataContract]
	public class ModelFault
	{
		private ErrorCode code = ErrorCode.Unknown;		
		private long globalId = 0;
		private string message = String.Empty;		

		public ModelFault()
		{
		}

		public ModelFault(ModelException modelException)
		{
			
			globalId = modelException.GlobalID;
			code = modelException.Code;
			message = modelException.Message;			
		}

		public ModelFault(Exception exception)
		{			
			message = exception.Message;
		}

		public ModelFault(string message)
		{
			this.message = message;
		}

		public ModelFault(ModelFault modelFault)
		{			
			globalId = modelFault.GlobalID;
			code = modelFault.Code;
			message = modelFault.Message;			
		}

		[DataMember]
		public ErrorCode Code
		{
			get
			{
				return code;
			}

			set
			{
				code = value;
			}
		}
		
		[DataMember]
		public string Message
		{
			get 
			{
				return message; 
			}

			set 
			{ 
				message = value;
			}
		}

		[DataMember]
		public long GlobalID
		{
			get
			{
				return globalId;
			}

			set
			{
				globalId = value;
			}
		}		
	}
	
	public class ModelException : Exception
	{
		private ErrorCode code = ErrorCode.Unknown;		
		private long globalId = 0;		

		public ModelException() : base(string.Empty)
		{
		}

		public ModelException(string message) : base(message)
		{
		}

		public ModelException(ErrorCode code, string message)
			: base(message)
		{
			this.code = code;
		}
	
		public ModelException(ErrorCode code, long globalId, string message)
			: base(message)
		{
			this.code = code;			
			this.globalId = globalId;
		}

		public ModelException(ErrorCode code, long globalId, long localId, string message)
			: base(message)
		{
			this.code = code;			
			this.globalId = globalId;
		}

		[DataMember]
		public long GlobalID
		{
			get
			{
				return globalId;
			}

			set
			{
				globalId = value;
			}
		}

		[DataMember]
		public ErrorCode Code
		{
			get
			{
				return code;
			}

			set
			{
				code = value;
			}
		}
	}
}
