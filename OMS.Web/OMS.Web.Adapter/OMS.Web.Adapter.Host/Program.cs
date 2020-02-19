using Outage.Common;
using System;

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
