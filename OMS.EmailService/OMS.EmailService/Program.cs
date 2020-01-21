#define IDLE_SCAN
//#define MANUAL_SCAN

using System;
using System.Collections.Generic;
using OMS.Email.Imap;
using OMS.Email.Interfaces;
using OMS.Email.Models;

namespace OMS.EmailService
{
    class Program
    {
        static void Main(string[] args)
        {
            // todo: add factory for email clients
            IImapEmailMapper mapper = new ImapEmailMapper();

            #if (IDLE_SCAN)
            #region Idle scan

            Console.WriteLine("Idle scanning starting...");

            // Use-case #1: Idle all-time listening to new messages
            IIdleEmailClient idleEmailClient = new ImapIdleEmailClient(mapper);

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


            #endregion
            #endif

            #if (MANUAL_SCAN)
            #region Manual scan

                        Console.WriteLine("Manual scan starting ...");
                        // Use-case #2: Manual scan for unread emails that can be setup as a cron job
                        IEmailClient emailClient = new ImapEmailClient(mapper);

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
            #endregion
            #endif

            Console.ReadLine();
        }
    }
}
