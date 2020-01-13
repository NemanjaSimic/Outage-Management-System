using Outage.Common;
using PubSubEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubSubEngineHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ILogger Logger = LoggerWrapper.Instance;

            try
            {
                string message = "Starting PubSub Engine...";
                Logger.LogInfo(message);
                Console.WriteLine("\n{0}\n", message);

                using (PubSubEngine.PubSubEngine pubsub = new PubSubEngine.PubSubEngine())
                {
                    pubsub.Start();

                    message = "Press <Enter> to stop the service.";
                    Console.WriteLine(message);
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("PubSub Engine failed.");
                Console.WriteLine(ex.StackTrace);
                Logger.LogError($"PubSub Engine failed.{Environment.NewLine}Message: {ex.Message} ", ex);
                Console.ReadLine();
            }
        }
    }
}
