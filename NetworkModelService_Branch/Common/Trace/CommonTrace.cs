using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace FTN.Common
{
	public class CommonTrace
	{
		private static TraceSwitch traceSwitch = new TraceSwitch("TraceLevel", "Trace lelvel for Model Service");

		public static TraceLevel TraceLevel
		{
			get
			{				
				return traceSwitch.Level;
			}
		}

		public static bool TraceError
		{
			get
			{
				return traceSwitch.TraceError;
			}
		}

		public static bool TraceInfo
		{
			get
			{
				return traceSwitch.TraceInfo;
			}
		}

		public static bool TraceVerbose
		{
			get
			{
				return traceSwitch.TraceVerbose;
			}
		}

		public static bool TraceWarning
		{
			get
			{
				return traceSwitch.TraceWarning;
			}
		}

		public static void WriteTrace(bool doTrace, string message)
		{
			Trace.WriteLineIf(doTrace, string.Format("{0} : {1}", DateTime.Now, message));
		}

		public static void WriteTrace(bool doTrace, string message, object arg1)
		{
			Trace.WriteLineIf(doTrace, string.Format("{0} : {1}", DateTime.Now, string.Format(message, arg1)));
		}

		public static void WriteTrace(bool doTrace, string message, object arg1, object arg2)
		{
			Trace.WriteLineIf(doTrace, string.Format("{0} : {1}", DateTime.Now, string.Format(message, arg1, arg2)));
		}

		public static void WriteTrace(bool doTrace, string message, object arg1, object arg2, object arg3)
		{
			Trace.WriteLineIf(doTrace, string.Format("{0} : {1}", DateTime.Now, string.Format(message, arg1, arg2, arg3)));
		}

		public static void WriteTrace(bool doTrace, string message, params object[] args)
		{
			Trace.WriteLineIf(doTrace, string.Format("{0} : {1}", DateTime.Now, string.Format(message, args)));
		}
	}
}
