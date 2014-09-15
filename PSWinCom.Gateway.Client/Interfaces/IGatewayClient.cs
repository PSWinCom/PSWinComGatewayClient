using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PSWinCom.Gateway.Client
{
    public interface IGatewayClient
    {
        int BatchSize { get; set; }
        string Username { get; set; }
        string Password { get; set; }
        ITransport Transport { get; set; }
        GatewayResponse Send(IEnumerable<Message> messages);
        GatewayResponse Send(params Message[] messages);
    }
}
