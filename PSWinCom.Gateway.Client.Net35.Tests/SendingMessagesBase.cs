using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PSWinCom.Gateway.Client.Tests
{
    public class SendingMessagesBase
    {
        protected GatewayClient client;
        protected XDocument last_request_xml;
        protected Moq.Mock<ITransport> mockTransport;

        [SetUp]
        public void Setup()
        {
            mockTransport = new Moq.Mock<ITransport>();
            client = new GatewayClient(mockTransport.Object);
            last_request_xml = new XDocument();
            mockTransport.Setup((t) => t.Send(It.IsAny<XDocument>())).Returns<XDocument>((xml) =>
            {
                last_request_xml = xml;
                return new TransportResult() { Content = new XDocument() };
            });
        }
        
        protected void Transport_returns_ok_for_all_messages()
        {
            mockTransport
                .Setup((t) => t.Send(It.IsAny<XDocument>()))
                .Returns<XDocument>((payload) => new TransportResult { 
                    Content = 
                        new XDocument(
                            new XElement("SESSION", 
                                new XElement("LOGON", "OK"), 
                                new XElement("MSGLST", payload.Descendants("MSG")
                                    .Select(msg => 
                                        new XElement("MSG", 
                                            new XElement("ID", msg.Element("ID").Value), 
                                            new XElement("STATUS", "OK")
                                        )
                                    )
                                )
                            )
                        )
                });
        }

        protected void Transport_returns(params XElement[] results)
        {
            mockTransport
                .Setup((t) => t.Send(It.IsAny<XDocument>()))
                .Returns(new TransportResult { 
                    Content = 
                        new XDocument(
                            new XElement("SESSION", 
                                new XElement("LOGON", "OK"),
                                new XElement("MSGLST", results)
                            )
                        )
                });
        }

        protected void Transport_returns_batch_status(string status, string description, params XElement[] results)
        {
            mockTransport
                .Setup((t) => t.Send(It.IsAny<XDocument>()))
                .Returns(new TransportResult
                {
                    Content =
                        new XDocument(
                            new XElement("SESSION",
                                new XElement("LOGON", status),
                                new XElement("REASON", description),
                                new XElement("MSGLST", results)
                            )
                        )
                });
        }

        protected static XElement message_result(string numInSession, string status)
        {
            return new XElement("MSG", new XElement("ID", numInSession), new XElement("STATUS", status));
        }
    }
}
