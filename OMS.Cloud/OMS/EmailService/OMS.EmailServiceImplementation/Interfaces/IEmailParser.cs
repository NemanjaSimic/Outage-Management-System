using OMS.EmailImplementation.Models;

namespace OMS.EmailImplementation.Interfaces
{
    public interface IEmailParser
	{
		OutageTracingModel Parse(OutageMailMessage message);
	}
}
