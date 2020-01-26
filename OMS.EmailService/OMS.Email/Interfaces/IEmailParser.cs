
namespace OMS.Email.Interfaces
{
    using OMS.Email.Models; 

    public interface IEmailParser
    {
        OutageTracingModel Parse(OutageMailMessage message);
    }
}
