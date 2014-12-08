using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PSWinCom.Gateway.Receiver
{
    public abstract class IncomingMessageHandler
    {
        private const string FAIL = "FAIL";
        private const string OK = "OK";
        public abstract bool OnIncomingMessageReceived(IncomingMessage message);

        protected XDocument HandleRequest(XDocument request)
        {
            return new XDocument(
                new XElement("MSGLST",
                    request.Descendants("MSG").Select(m => {
                        string status;
                        try
                        {
                            status = OnIncomingMessageReceived(ParseMessage(m)) ? OK : FAIL;
                        }
                        catch (Exception ex)
                        {
                            status = FAIL;
                        }

                        return new XElement("MSG",
                            m.Element("ID"),
                            new XElement("STATUS",
                                status
                            )
                        );
                    })
                )
            );
        }

        private static IncomingMessage ParseMessage(XElement m)
        {
            return new IncomingMessage { 
                From = m.Element("SND").Value, 
                To = m.Element("RCV").Value, 
                Text = m.Element("TEXT").Value
            };
        }
    }
}
