using OMS.EmailService.Imap;
using System;

namespace OMS.EmailService
{
    class Program
    {
        static void Main(string[] args)
        {
            IEmailClient emailClient = new ImapMailClient();
            if (emailClient.Connect())
            {
                Console.WriteLine("Connected email client");
            }
            else
            {
                Console.WriteLine("Could not connect email client.");
            }

            Console.ReadLine();
        }
    }
}
