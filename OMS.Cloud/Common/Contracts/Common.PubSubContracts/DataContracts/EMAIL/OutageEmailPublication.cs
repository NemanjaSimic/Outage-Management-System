using OMS.Common.Cloud;
using OMS.Common.PubSubContracts.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.PubSubContracts.DataContracts.EMAIL
{
	public class OutageEmailPublication : Publication
	{
		public OutageEmailPublication(Topic topic, EmailToOutageMessage message) : base(topic, message) { }
	}
}
