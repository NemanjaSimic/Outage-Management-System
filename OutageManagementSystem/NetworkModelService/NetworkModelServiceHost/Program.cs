﻿using Outage.Common;
using System;

namespace Outage.NetworkModelServiceHost
{
	public class Program
    {
        private static void Main(string[] args)
        {
            ILogger logger = LoggerWrapper.Instance;

            try
            {                
                string message = "Starting Network Model Service...";
                logger.LogInfo(message);
                Console.WriteLine("\n{0}\n", message);

                using (NetworkModelService.NetworkModelService nms = new NetworkModelService.NetworkModelService())
                {
                    nms.Start();

                    message = "Press <Enter> to stop the service.";
                    Console.WriteLine(message);
                    Console.ReadLine();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Network Model Service failed.");
                Console.WriteLine(ex.StackTrace);
                logger.LogError($"Network Model Service failed.{Environment.NewLine}Message: {ex.Message} ", ex);
                Console.ReadLine();
            }
        }
    }
}
