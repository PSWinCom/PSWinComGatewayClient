using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PSWinCom.Gateway.Client
{
    public class MessageClient : IMessageClient
    {
        public string Username { get; set; }
        public string Password { get; set; }

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
            var result = new SendResult();
            XDocument doc = new XDocument();
            doc.Add(
                new XElement("SESSION",
                    new XElement("CLIENT", Username),
                    new XElement("PW", Password),
                    new XElement("MSGLST",
                        GetMessageElements(messages)
                    )
                )
            );
            var userReferences = messages.ToDictionary((m) => m.NumInSession, m => m.UserReference);

            var transportResult = Transport.Send(doc);

            result.Results =
                transportResult
                    .Content
                    .Descendants("MSG")
                    .Select((el) => new MessageResult
                    {
                        UserReference = userReferences[int.Parse(el.Element("ID").Value)],
                        Status = el.Element("STATUS").Value
                    });

            return result;
        }

        private IEnumerable<XElement> GetMessageElements(IEnumerable<Message> messages)
        {
            var numInSession = 1;
            foreach (var msg in messages)
            {
                msg.NumInSession = numInSession++;
                XElement msgElement = 
                    new XElement("MSG",
                        GetMessagePropertyElements(msg)
                    );
                yield return msgElement;
            }
        }

        private IEnumerable<XElement> GetMessagePropertyElements(Message msg)
        {
            yield return new XElement("ID", msg.NumInSession);
            yield return new XElement("TEXT", msg.Text);
            yield return new XElement("SND", msg.SenderNumber);
            yield return new XElement("RCV", msg.ReceiverNumber);
            if (msg.Tariff > 0)
                yield return new XElement("TARIFF", msg.Tariff);
        }
    }
}
