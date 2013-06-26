﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSWinCom.Gateway.Client
{
    public interface IAsyncMessageClient : IMessageClient
    {
        Task<SendResult> SendAsync(IEnumerable<Message> messages);
    }
}
