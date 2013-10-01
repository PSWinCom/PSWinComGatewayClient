using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PSWinCom.Gateway.Client
{
    public interface IGatewayClient
    {
        string Username { get; set; }
        string Password { get; set; }
        ITransport Transport { get; set; }
        SendResult Send(IEnumerable<Message> messages);
    }
}
