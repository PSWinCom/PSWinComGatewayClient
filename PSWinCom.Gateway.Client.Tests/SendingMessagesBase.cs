using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSWinCom.Gateway.Client.Tests
{
    public class SendingMessagesBase
    {
        protected GatewayClient client;
        protected System.Xml.Linq.XDocument last_request_xml;
        protected Moq.Mock<ITransport> mockTransport;

        [SetUp]
        public void Setup()
        {
            mockTransport = new Moq.Mock<ITransport>();
            client = new GatewayClient(mockTransport.Object);
            last_request_xml = new System.Xml.Linq.XDocument();
            mockTransport.Setup((t) => t.Send(It.IsAny<System.Xml.Linq.XDocument>())).Returns<System.Xml.Linq.XDocument>((xml) =>
            {
                last_request_xml = xml;
                return new TransportResult() { Content = new System.Xml.Linq.XDocument() };
            });
        }
        
        protected void Transport_returns_ok_for_all_messages()
        {
            mockTransport.Setup((t) => t.Send(It.IsAny<System.Xml.Linq.XDocument>())).Returns<System.Xml.Linq.XDocument>((payload) => new TransportResult { Content = new System.Xml.Linq.XDocument(new System.Xml.Linq.XElement("SESSION", new System.Xml.Linq.XElement("MSGLST", payload.Descendants("MSG").Select(msg => new System.Xml.Linq.XElement("MSG", new System.Xml.Linq.XElement("ID", msg.Element("ID").Value), new System.Xml.Linq.XElement("STATUS", "OK")))))) });
        }

        protected void Transport_returns(params System.Xml.Linq.XElement[] results)
        {
            mockTransport.Setup((t) => t.Send(It.IsAny<System.Xml.Linq.XDocument>())).Returns(new TransportResult { Content = new System.Xml.Linq.XDocument(new System.Xml.Linq.XElement("SESSION", new System.Xml.Linq.XElement("MSGLST", results))) });
        }

        protected static System.Xml.Linq.XElement message_result(string numInSession, string status)
        {
            return new System.Xml.Linq.XElement("MSG", new System.Xml.Linq.XElement("ID", numInSession), new System.Xml.Linq.XElement("STATUS", status));
        }
    }
}
