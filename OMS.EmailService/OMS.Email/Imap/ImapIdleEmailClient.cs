using ImapX;
using System;
using OMS.Email.Models;
using OMS.Email.Interfaces;

namespace OMS.Email.Imap
{
    public class ImapIdleEmailClient : ImapEmailClient, IIdleEmailClient
    {
        public ImapIdleEmailClient(IImapEmailMapper mapper) : base(mapper) { }

        public bool StartIdling()
        {
            Folder folder = _client.Folders["INBOX"];
            return folder.StartIdling();
        }

        public void RegisterIdleHandler()
        {
            _client.OnNewMessagesArrived += OnMessageArrived;            
        }

        private void OnMessageArrived(object sender, IdleEventArgs args)
        {
            foreach(var message in args.Messages)
            {
                OutageMailMessage outageMessage = _mapper.MapMail(message);
                Console.WriteLine(outageMessage);

                // Todo: Send message to parser or Gid to another service here 
            }
        }
    }
}
