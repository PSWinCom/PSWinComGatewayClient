using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Should;
using Moq;
using System.Xml.Linq;

namespace PSWinCom.Gateway.Client.Tests
{
    [TestFixture]
    public class SendingMessages : SendingMessagesBase
    {

        [Test, ExpectedException(typeof(ArgumentException))]
        public void Should_not_accept_an_empty_list_of_messages()
        {
            client.Send(new Sms[] { });
        }

        [Test]
        public void Should_include_username_and_password()
        {
            client.Username = "test";
            client.Password = "pass";

            client.Send(new Sms[] { new Sms("26112", "12345678", "A message") });

            last_request_xml.Root.Name.ShouldEqual("SESSION");
            last_request_xml.Root.Element("CLIENT").Value.ShouldEqual("test");
            last_request_xml.Root.Element("PW").Value.ShouldEqual("pass");
        }

        [Test]
        public void Should_build_message_list_with_minimum_information()
        {
            client.Send(
                new[] { 
                    new Sms { Text = "some text", ReceiverNumber = "4799999999", SenderNumber = "tester" },
                    new Sms { Text = "some text 2", ReceiverNumber = "4799999998", SenderNumber = "tester2" } 
                }
            );

            last_request_xml.Root.Name.ShouldEqual("SESSION");
            last_request_xml.Root.Element("MSGLST").ShouldNotBeNull();

            var elements = last_request_xml.Root.Element("MSGLST").Elements("MSG");
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
        public void Should_support_all_possible_message_properties()
        {
            client.Send(
                new[] { 
                    new Sms { 
                        Text = "some text", 
                        ReceiverNumber = "4799999999", 
                        SenderNumber = "tester",
                        RequestReceipt = true,
                        Tariff = 100,
                        Network = "012:03",
                        TimeToLive = TimeSpan.FromMinutes(60.3),
                        CpaTag = "Something",
                        AgeLimit = 17,
                        ShortCode = "2027",
                        ServiceCode = "10001",
                        DeliveryTime = new DateTime(2099, 12, 31, 23, 59, 59),
                        Replace = Replace.Set7,
                        IsFlashMessage = true,
                        Type = MessageType.vCard,
                    },
                }
            );

            var message = last_request_xml.Root.Element("MSGLST").Elements("MSG").First();

            message.Element("TEXT").Value.ShouldEqual("some text");
            message.Element("SND").Value.ShouldEqual("tester");
            message.Element("RCV").Value.ShouldEqual("4799999999");
            message.Element("RCPREQ").Value.ShouldEqual("Y");
            message.Element("TARIFF").Value.ShouldEqual("100");
            message.Element("NET").Value.ShouldEqual("012:03");
            message.Element("TTL").Value.ShouldEqual("60");
            message.Element("CPATAG").Value.ShouldEqual("Something");
            message.Element("AGELIMIT").Value.ShouldEqual("17");
            message.Element("SHORTCODE").Value.ShouldEqual("2027");
            message.Element("SERVICECODE").Value.ShouldEqual("10001");
            message.Element("DELIVERYTIME").Value.ShouldEqual("209912312359");
            message.Element("REPLACE").Value.ShouldEqual("7");
            message.Element("CLASS").Value.ShouldEqual("0");
            message.Element("OP").Value.ShouldEqual("6");
        }

        [Test]
        public void Should_support_all_appropriate_properties_when_sending_mms()
        {
            client.Send(
                new[] { 
                    new Mms { 
                        Text = "some text", 
                        ReceiverNumber = "4799999999", 
                        SenderNumber = "tester",
                        RequestReceipt = true,
                        Tariff = 100,
                        TimeToLive = TimeSpan.FromMinutes(60.3),
                        CpaTag = "Something",
                        ShortCode = "2027",
                        DeliveryTime = new DateTime(2099, 12, 31, 23, 59, 59),
                        MmsData = System.Text.Encoding.UTF8.GetBytes("Test zip data")
                    },
                    new Mms {
                        Text = "some text", 
                        ReceiverNumber = "4799999999", 
                        SenderNumber = "tester",
                        RequestReceipt = true,
                        Tariff = 100,
                        TimeToLive = TimeSpan.FromMinutes(60.3),
                        CpaTag = "Something",
                        ShortCode = "2027",
                        DeliveryTime = new DateTime(2099, 12, 31, 23, 59, 59),
                    }
                }
            );

            var message = last_request_xml.Root.Element("MSGLST").Elements("MSG").First();

            message.Element("TEXT").Value.ShouldEqual("some text");
            message.Element("SND").Value.ShouldEqual("tester");
            message.Element("RCV").Value.ShouldEqual("4799999999");
            message.Element("RCPREQ").Value.ShouldEqual("Y");
            message.Element("TARIFF").Value.ShouldEqual("100");
            message.Element("TTL").Value.ShouldEqual("60");
            message.Element("CPATAG").Value.ShouldEqual("Something");
            message.Element("SHORTCODE").Value.ShouldEqual("2027");
            message.Element("DELIVERYTIME").Value.ShouldEqual("209912312359");
            message.Element("OP").Value.ShouldEqual("13");
            message.Element("MMSFILE").Value.ShouldEqual("VGVzdCB6aXAgZGF0YQ==");

            last_request_xml.Root.Element("MSGLST").Elements("MSG").Last().Element("MMSFILE").ShouldBeNull();
        }

        [Test, Ignore("Removed requirement, not really needed")]
        public void Should_have_DOCTYPE()
        {
            client.Send(
                new[] { 
                    new Sms { Text = "some text", ReceiverNumber = "4799999999", SenderNumber = "tester" },
                    new Sms { Text = "some text 2", ReceiverNumber = "4799999998", SenderNumber = "tester2" } 
                }
            );
            last_request_xml.FirstNode.NodeType.ShouldEqual(System.Xml.XmlNodeType.DocumentType);
            var type = last_request_xml.FirstNode as XDocumentType;
            type.Name.ShouldEqual("SESSION");
            type.SystemId.ShouldEqual("pswincom_submit.dtd");
        }

        [Test]
        public void Should_set_encoding_in_declaration()
        {
            client.Send(
                new[] { 
                    new Sms { Text = "some text", ReceiverNumber = "4799999999", SenderNumber = "tester" },
                    new Sms { Text = "some text 2", ReceiverNumber = "4799999998", SenderNumber = "tester2" } 
                }
            );
            last_request_xml.Declaration.Encoding.ShouldEqual("iso-8859-1");
        }

        [Test]
        public void Should_support_tariff()
        {
            client.Send(
                new[] { 
                    new Sms { 
                        Tariff = 100,
                        Text = "some text", 
                        ReceiverNumber = "4799999999", 
                        SenderNumber = "tester" 
                    },
                }
            );

            last_request_xml.Descendants("MSG").First().Element("TARIFF").Value.ShouldEqual("100");
        }

        [Test]
        public void Should_assign_num_in_session()
        {
            var msg1 = new Sms();
            var msg2 = new Sms();

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
        public void Should_return_status_on_messages_using_userref_if_possible()
        {
            var msg1 = new Sms { UserReference = "message1" };
            var msg2 = new Sms { UserReference = "message2" };

            Transport_returns(
                message_result("2", "OK"), 
                message_result("1", "FAIL"));

            var response = client.Send(new[] {
                msg1,
                msg2
            });

            response.Results.Count().ShouldEqual(2);
            response.Results.First((m) => m.UserReference == "message1").Status.ShouldEqual("FAIL");
            response.Results.First((m) => m.UserReference == "message2").Status.ShouldEqual("OK");
        }
    }

    [TestFixture]
    public class BatchedSending : SendingMessagesBase
    {
        [Test]
        public void Test()
        {
            client.BatchSize = 20;
            Transport_returns_ok_for_all_messages();
            var result = client.Send(100.Smses());
            mockTransport.Verify(t => t.Send(It.IsAny<XDocument>()), Times.Exactly(5));
            result.Results.Count().ShouldEqual(100);
        }
    }

    public static class TestHelpers
    {
        public static IEnumerable<Message> Smses(this int count) {
            for (var i = 0; i < count; i++)
            {
                yield return new Sms("26112", "12345678", "Message " + i.ToString());
            }
        }
    }
}
