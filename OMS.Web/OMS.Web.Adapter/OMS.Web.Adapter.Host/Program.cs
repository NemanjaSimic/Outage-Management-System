using OMS.Web.Adapter.Outage;
using OMS.Web.Adapter.SCADA;
using OMS.Web.Adapter.Topology;
using OMS.Web.Common;
using OMS.Web.Common.Mappers;
using Outage.Common;
using Outage.Common.ServiceProxies.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OMS.Web.Adapter.Host
{
    public class Program
    {
        private static void Main(string[] args)
        {
            ILogger logger = LoggerWrapper.Instance;

            try
            {
                string message = "Starting Adapter...";
                logger.LogInfo(message);
                Console.WriteLine("\n{0}\n", message);

                using (Adapter adapter = new Adapter())
                {
                    adapter.Start();

                    message = "Press <Enter> to stop the service.";
                    Console.WriteLine(message);
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Adapter failed.");
                Console.WriteLine(ex.StackTrace);
                logger.LogError($"Adapter failed.{Environment.NewLine}Message: {ex.Message} ", ex);
                Console.ReadLine();
            }
        }
   
    }
}
