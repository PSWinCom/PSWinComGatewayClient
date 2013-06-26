using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PSWinCom.Gateway.Client
{
    public class MessageClient : MessageClientBase, IMessageClient
    {

        public MessageClient(ITransport transport)
        {
            Transport = transport;
        }

        public override SendResult Send(IEnumerable<Message> messages)
        {
            XDocument doc;
            doc = BuildPayload(messages);
            var transportResult = Transport.Send(doc);
            return GetSendResult(messages, transportResult);
        }
    }
}
