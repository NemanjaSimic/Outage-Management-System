using Outage.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Outage.DataImporter.CIMAdapter;

namespace Outage.NetworkModelServiceHost
{
    public class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                CIMAdapterClass cim = new CIMAdapterClass();
                
                string message = "Starting Network Model Service...";
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
                Console.WriteLine("\n{0}\n", message);

                using (NetworkModelService.NetworkModelService nms = new NetworkModelService.NetworkModelService())
                {
                    nms.Start();

                    message = "Press <Enter> to stop the service.";
                    CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
                    Console.WriteLine(message);
                    Console.ReadLine();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("NetworkModelService failed.");
                Console.WriteLine(ex.StackTrace);
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, "NetworkModelService failed.");
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.StackTrace);
                Console.ReadLine();
            }
        }
    }
}
