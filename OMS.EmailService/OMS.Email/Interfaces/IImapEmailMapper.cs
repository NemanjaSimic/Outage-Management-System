<<<<<<< HEAD
﻿using ImapX;
using OMS.Email.Models;

namespace OMS.Email.Interfaces
{
=======
﻿namespace OMS.Email.Interfaces
{
    using ImapX;
    using OMS.Email.Models;
    
>>>>>>> 2800298cec0dac58b6c9a650c22ac579428c4bc6
    public interface IImapEmailMapper
    {
        OutageMailMessage MapMail(Message message);
    }
}
