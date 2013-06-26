using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PSWinCom.Gateway.Client
{
    public class MessageClient : MessageClientBase, IMessageClient
    {

        public ITransport Transport { get; set; }

        public MessageClient(ITransport transport)
        {
            Transport = transport;
        }

        public static IMessageClient GetHttpClient()
        {
            return GetHttpClient("https://sms3.pswin.com/sms");
        }

        public static IMessageClient GetHttpClient(string url) {
            return new MessageClient(new HttpTransport(new Uri(url)));
        }

        public SendResult Send(IEnumerable<Message> messages)
        {
            XDocument doc;
            doc = BuildPayload(messages);
            var transportResult = Transport.Send(doc);
            return GetSendResult(messages, transportResult);
        }
    }
}
