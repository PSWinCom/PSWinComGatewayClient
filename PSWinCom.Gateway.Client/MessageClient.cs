using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PSWinCom.Gateway.Client
{
    public class MessageClient
    {
        public MessageClient()
        {

        }

        public string Username { get; set; }
        public string Password { get; set; }

        public Transport Transport { get; set; }

        public void Send(IEnumerable<Message> messages)
        {
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
            Transport.Send(doc);
        }
        private IEnumerable<XElement> GetMessageElements(IEnumerable<Message> messages)
        {
            foreach (var msg in messages)
            {
                yield return new XElement("MSG",
                    new XElement("TEXT", msg.Text),
                    new XElement("SND", msg.Sender),
                    new XElement("RCV", msg.Recipient)
                );
            }
        }
    }
}
