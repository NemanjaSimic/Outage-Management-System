namespace OMS.EmailService
{
    using OMS.Email.Factories;
    using OMS.Email.Interfaces;
    using OMS.Email.Models;
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    class Program
    {
        private static string IDLE_SCAN = "IDLE_SCAN";
        private static string MANUAL_SCAN = "MANUAL_SCAN";

        private static Dictionary<string, Action> ScanActions = new Dictionary<string, Action>
        {
            { IDLE_SCAN, StartIdleScan },
            { MANUAL_SCAN, StartManualScan },
            { string.Empty , () => Console.WriteLine("No scanType defined in App.config.") }
        };

        static void Main(string[] args)
        {
            string scanType = ConfigurationManager.AppSettings.Get("scanType")?.ToUpper() ?? string.Empty;

            ScanActions[scanType].Invoke();

            Console.ReadLine();
        }

        private static void StartManualScan()
        {
            Console.WriteLine("Manual scan starting ...");

            IEmailClient emailClient = new ImapClientFactory().CreateClient();

            if (!emailClient.Connect())
            {
                Console.WriteLine("Could not connect email client.");
                Console.ReadLine();
                return;
            }

            IEnumerable<OutageMailMessage> unreadMessages = emailClient.GetUnreadMessages();

            foreach (var message in unreadMessages)
                Console.WriteLine(message);

            Console.WriteLine("Manual scan finished.");
        }

        private static void StartIdleScan()
        {
            // Use-case #1: Idle all-time listening to new messages
            IIdleEmailClient idleEmailClient = new ImapIdleClientFactory().CreateClient();

            Console.WriteLine("Idle scanning starting...");
            if (!idleEmailClient.Connect())
            {
                Console.WriteLine("Could not connect email client.");
                Console.ReadLine();
                return;
            }

            idleEmailClient.RegisterIdleHandler();
            if (!idleEmailClient.StartIdling())
            {
                Console.WriteLine("Could not start idling.");
            }

            Console.WriteLine("Idle scanning started.");
        }

    }
}
