using System;
using System.Collections.Generic;
using System.Text;

namespace PSWinCom.Gateway.Client
{
    public class SendResult
    {
        public IEnumerable<MessageResult> Results { get; set; }
    }
}