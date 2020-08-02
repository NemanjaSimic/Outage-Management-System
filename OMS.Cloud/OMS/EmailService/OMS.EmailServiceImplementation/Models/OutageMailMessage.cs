using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.CallTrackingServiceImplementation.Models
{
	public class OutageMailMessage
	{
		public string SenderDisplayName { get; set; }
		public string SenderEmail { get; set; }
		public string Body { get; set; }

		public override string ToString()
		{
			return $"From: {SenderDisplayName} <{SenderEmail}> sent: {Body}";
		}
	}
}
