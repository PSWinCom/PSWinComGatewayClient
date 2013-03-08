using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Should;
using Moq;
using System.Xml.Linq;

namespace PSWinCom.Gateway.Client.Tests
{
    [TestFixture]
    public class ClientUsesTransport
    {
        private Mock<Transport> mockTransport;
        private MessageClient client;
        private XDocument last_doc;

        [Test]
        public void Should_send_xml_to_transport()
        {
            var client = new MessageClient();
            var mockTransport = new Mock<Transport>();
            client.Transport = mockTransport.Object;

            client.Send(new Message[] { });

            mockTransport.Verify((t) => t.Send(It.IsAny<XDocument>()), Times.Once());
        }

        [Test]
        public void Should_include_username_and_password()
        {
            client.Username = "test";
            client.Password = "pass";

            client.Send(new Message[] { });

            last_doc.Root.Name.ShouldEqual("SESSION");
            last_doc.Root.Element("CLIENT").Value.ShouldEqual("test");
            last_doc.Root.Element("PW").Value.ShouldEqual("pass");
        }

        [Test]
        public void Should_build_message_list()
        {
            client.Username = "test";
            client.Password = "pass";

            client.Send(
                new[] { 
                    new Message { Text = "some text", Recipient = "4799999999", Sender = "tester" },
                    new Message { Text = "some text 2", Recipient = "4799999998", Sender = "tester2" } 
                }
            );

            last_doc.Root.Name.ShouldEqual("SESSION");
            last_doc.Root.Element("MSGLST").ShouldNotBeNull();

            var elements = last_doc.Root.Element("MSGLST").Elements("MSG");
            elements.Count().ShouldEqual(2);
            elements.First().Element("TEXT").Value.ShouldEqual("some text");
            elements.First().Element("SND").Value.ShouldEqual("tester");
            elements.First().Element("RCV").Value.ShouldEqual("4799999999");
        }

        [SetUp]
        public void Setup()
        {
            client = new MessageClient();
            mockTransport = new Mock<Transport>();
            client.Transport = mockTransport.Object;

            last_doc = new XDocument();

            mockTransport
                .Setup((t) => t.Send(It.IsAny<XDocument>()))
                .Returns<XDocument>((xml) =>
                {
                    last_doc = xml;
                    return new SendResult();
                });
        }
    }
}
