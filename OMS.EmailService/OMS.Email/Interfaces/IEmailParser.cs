<<<<<<< HEAD
﻿using OMS.Email.Models;

namespace OMS.Email.Interfaces
{
=======
﻿
namespace OMS.Email.Interfaces
{
    using OMS.Email.Models; 

>>>>>>> 2800298cec0dac58b6c9a650c22ac579428c4bc6
    public interface IEmailParser
    {
        OutageTracingModel Parse(OutageMailMessage message);
    }
}
