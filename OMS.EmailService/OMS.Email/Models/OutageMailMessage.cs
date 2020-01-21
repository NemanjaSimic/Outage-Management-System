namespace OMS.Email.Models
{
    public class OutageMailMessage
    {
        // expandable
        public string SenderDisplayName { get; set; }
        public string SenderEmail { get; set; }
        public string Body { get; set; }

        public override string ToString()
        {
            return $"From: {SenderDisplayName} <{SenderEmail}> sent: {Body}";
        }
    }
}
