using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PSWinCom.Gateway.Client
{
    public abstract class GatewayClientBase : IGatewayClient
    {
        public ITransport Transport { get; set; }

        public GatewayClientBase(ITransport transport)
        {
            Transport = transport;
        }

        public SendResult Send(params Message[] messages)
        {
            return Send(messages.AsEnumerable());
        }

        public virtual SendResult Send(IEnumerable<Message> messages)
        {
            var transportResult = Transport.Send(BuildPayload(messages));
            return GetSendResult(messages, transportResult);
        }

        protected XDocument BuildPayload(IEnumerable<Message> messages)
        {
            XDocument doc;
            doc = new XDocument(new XDeclaration("1.0", "iso-8859-1", null));
            doc.Add(
                new XDocumentType("SESSION", null, "pswincom_submit.dtd", null), 
                new XElement("SESSION", 
                    new XElement("CLIENT", Username), 
                    new XElement("PW", Password), 
                    new XElement("MSGLST", 
                        GetMessageElements(messages))));
            return doc;
        }

        private IEnumerable<XElement> GetMessageElements(IEnumerable<Message> messages)
        {
            var numInSession = 1;
            foreach (var msg in messages)
            {
                msg.NumInSession = numInSession++;
                XElement msgElement = new XElement("MSG", GetMessagePropertyElements(msg));
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

        protected static SendResult GetSendResult(IEnumerable<Message> messages, TransportResult transportResult)
        {
            var result = new SendResult();
            var userReferences = messages.ToDictionary((m) => m.NumInSession, m => m.UserReference);
            result.Results = transportResult.Content.Descendants("MSG").Select((el) => new MessageResult { UserReference = userReferences[int.Parse(el.Element("ID").Value)], Status = el.Element("STATUS").Value });
            return result;
        }

        public string Password { get; set; }
        public string Username { get; set; }
    }
}
