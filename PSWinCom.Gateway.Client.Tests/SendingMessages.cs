using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Should;
using Moq;
using System.Xml.Linq;
using System.Net;

namespace PSWinCom.Gateway.Client.Tests
{
    [TestFixture]
    public class SendingMessages
    {
        private Mock<ITransport> mockTransport;
        private MessageClient client;
        private XDocument last_doc;

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
        public void Should_build_message_list_with_minimum_information()
        {
            client.Send(
                new[] { 
                    new Message { Text = "some text", ReceiverNumber = "4799999999", SenderNumber = "tester" },
                    new Message { Text = "some text 2", ReceiverNumber = "4799999998", SenderNumber = "tester2" } 
                }
            );

            last_doc.Root.Name.ShouldEqual("SESSION");
            last_doc.Root.Element("MSGLST").ShouldNotBeNull();

            var elements = last_doc.Root.Element("MSGLST").Elements("MSG");
            elements.Count().ShouldEqual(2);

            elements.First().Element("ID").Value.ShouldEqual("1");
            elements.First().Element("TEXT").Value.ShouldEqual("some text");
            elements.First().Element("SND").Value.ShouldEqual("tester");
            elements.First().Element("RCV").Value.ShouldEqual("4799999999");

            elements.Last().Element("ID").Value.ShouldEqual("2");
            elements.Last().Element("TEXT").Value.ShouldEqual("some text 2");
            elements.Last().Element("SND").Value.ShouldEqual("tester2");
            elements.Last().Element("RCV").Value.ShouldEqual("4799999998");
        }

        [Test]
        public void Should_support_tariff()
        {
            client.Send(
                new[] { 
                    new Message { 
                        Tariff = 100,
                        Text = "some text", 
                        ReceiverNumber = "4799999999", 
                        SenderNumber = "tester" 
                    },
                }
            );

            last_doc.Descendants("MSG").First().Element("TARIFF").Value.ShouldEqual("100");
        }

        [Test]
        public void Should_set_num_in_session()
        {
            var msg1 = new Message();
            var msg2 = new Message();

            client.Send(
                new[] { 
                    msg1,
                    msg2
                }
            );

            msg1.NumInSession.ShouldEqual(1);
            msg2.NumInSession.ShouldEqual(2);
        }

        [Test]
        public void Should_return_status_on_messages()
        {
            var msg1 = new Message { UserReference = "message1" };
            var msg2 = new Message { UserReference = "message2" };

            Transport_returns(
                message_result("2", "OK"), 
                message_result("1", "FAIL"));

            var result = client.Send(new[] {
                msg1,
                msg2
            });

            result.Results.Count().ShouldEqual(2);
            result.Results.First((m) => m.UserReference == "message1").Status.ShouldEqual("FAIL");
            result.Results.First((m) => m.UserReference == "message2").Status.ShouldEqual("OK");
        }

        private void Transport_returns(params XElement[] results)
        {
            mockTransport
                .Setup((t) => t.Send(It.IsAny<XDocument>()))
                .Returns(new TransportResult
                {
                    Content = new XDocument(
                        new XElement("SESSION",
                            new XElement("MSGLST",
                                results
                            )
                        )
                    )
                });
        }

        private static XElement message_result(string numInSession, string status)
        {
            return new XElement("MSG",
                new XElement("ID", numInSession),
                new XElement("STATUS", status)
            );
        }

        [SetUp]
        public void Setup()
        {
            mockTransport = new Mock<ITransport>();
            client = new MessageClient(mockTransport.Object);

            last_doc = new XDocument();

            mockTransport
                .Setup((t) => t.Send(It.IsAny<XDocument>()))
                .Returns<XDocument>((xml) =>
                {
                    last_doc = xml;
                    return new TransportResult()
                    {
                        Content = new XDocument()
                    };
                });
        }
    }
}
