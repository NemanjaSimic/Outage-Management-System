using OMS.Email.Models;

namespace OMS.Email.Interfaces
{
    public interface IEmailParser
    {
        OutageTracingModel Parse(OutageMailMessage message);
    }
}
