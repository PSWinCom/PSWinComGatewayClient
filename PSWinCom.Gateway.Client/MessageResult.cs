using System;
using System.Collections.Generic;
using System.Text;

namespace PSWinCom.Gateway.Client
{
    public class MessageResult
    {
        public string GatewayReference { get; set; }
        public string UserReference { get; set; }
        public string Status { get; set; }
    }
}
