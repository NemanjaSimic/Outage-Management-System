<<<<<<< HEAD
﻿using OMS.Email.Models;
using System.Collections.Generic;

namespace OMS.Email.Interfaces
{
=======
﻿namespace OMS.Email.Interfaces
{
    using OMS.Email.Models;
    using System.Collections.Generic;

>>>>>>> 2800298cec0dac58b6c9a650c22ac579428c4bc6
    public interface IEmailClient
    {
        bool Connect();
        IEnumerable<OutageMailMessage> GetUnreadMessages();
    }
}
