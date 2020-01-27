<<<<<<< HEAD
﻿#define IDLE_SCAN
//#define MANUAL_SCAN

using System;
using System.Collections.Generic;
using OMS.Email.Dispatchers;
using OMS.Email.EmailParsers;
using OMS.Email.Factories;
using OMS.Email.Imap;
using OMS.Email.Interfaces;
using OMS.Email.Models;

namespace OMS.EmailService
{
=======
﻿//#define IDLE_SCAN
#define MANUAL_SCAN

namespace OMS.EmailService
{
    using System;
    using System.Collections.Generic;
    using OMS.Email.Dispatchers;
    using OMS.Email.EmailParsers;
    using OMS.Email.Factories;
    using OMS.Email.Imap;
    using OMS.Email.Interfaces;
    using OMS.Email.Models;

>>>>>>> 2800298cec0dac58b6c9a650c22ac579428c4bc6
    class Program
    {
        static void Main(string[] args)
        {
            #if (IDLE_SCAN)          
            Console.WriteLine("Idle scanning starting...");

            // Use-case #1: Idle all-time listening to new messages
            IIdleEmailClient idleEmailClient = new ImapIdleClientFactory().CreateClient();

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
            #endif


            #if (MANUAL_SCAN)
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
            #endif

            Console.ReadLine();
        }
    }
}
