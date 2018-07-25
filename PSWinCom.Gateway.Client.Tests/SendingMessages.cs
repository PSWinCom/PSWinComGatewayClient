using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using FluentAssertions;
using Moq;
using System.Xml.Linq;

namespace PSWinCom.Gateway.Client.Tests
{
    [TestFixture]
    public class SendingMessages : SendingMessagesBase
    {

        [Test]
        public void Should_include_username_and_password()
        {
            client.Username = "test";
            client.Password = "pass";

            client.Send(new Sms[] { new Sms("26112", "12345678", "A message") });

            last_request_xml.Root.Name.Should().Be("SESSION");
            last_request_xml.Root.Element("CLIENT").Value.Should().Be("test");
            last_request_xml.Root.Element("PW").Value.Should().Be("pass");
            last_request_xml.Root.Element("SD").Should().BeNull();
        }

        [Test]
        public void Should_include_session_data_if_specified()
        {
            client.Send("My session data", new Sms("26112", "12345678", "A message"));

            last_request_xml.Root.Name.Should().Be("SESSION");
            last_request_xml.Root.Element("SD").Value.Should().Be("My session data");
            
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

            last_request_xml.Root.Name.Should().Be("SESSION");
            last_request_xml.Root.Element("MSGLST").Should().NotBeNull();

            var elements = last_request_xml.Root.Element("MSGLST").Elements("MSG");
            elements.Count().Should().Be(2);

            elements.First().Element("ID").Value.Should().Be("1");
            elements.First().Element("TEXT").Value.Should().Be("some text");
            elements.First().Element("SND").Value.Should().Be("tester");
            elements.First().Element("RCV").Value.Should().Be("4799999999");

            elements.Last().Element("ID").Value.Should().Be("2");
            elements.Last().Element("TEXT").Value.Should().Be("some text 2");
            elements.Last().Element("SND").Value.Should().Be("tester2");
            elements.Last().Element("RCV").Value.Should().Be("4799999998");
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

            message.Element("TEXT").Value.Should().Be("some text");
            message.Element("SND").Value.Should().Be("tester");
            message.Element("RCV").Value.Should().Be("4799999999");
            message.Element("RCPREQ").Value.Should().Be("Y");
            message.Element("TARIFF").Value.Should().Be("100");
            message.Element("NET").Value.Should().Be("012:03");
            message.Element("TTL").Value.Should().Be("60");
            message.Element("CPATAG").Value.Should().Be("Something");
            message.Element("AGELIMIT").Value.Should().Be("17");
            message.Element("SHORTCODE").Value.Should().Be("2027");
            message.Element("SERVICECODE").Value.Should().Be("10001");
            message.Element("DELIVERYTIME").Value.Should().Be("209912312359");
            message.Element("REPLACE").Value.Should().Be("7");
            message.Element("CLASS").Value.Should().Be("0");
            message.Element("OP").Value.Should().Be("6");
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

            message.Element("TEXT").Value.Should().Be("some text");
            message.Element("SND").Value.Should().Be("tester");
            message.Element("RCV").Value.Should().Be("4799999999");
            message.Element("RCPREQ").Value.Should().Be("Y");
            message.Element("TARIFF").Value.Should().Be("100");
            message.Element("TTL").Value.Should().Be("60");
            message.Element("CPATAG").Value.Should().Be("Something");
            message.Element("SHORTCODE").Value.Should().Be("2027");
            message.Element("DELIVERYTIME").Value.Should().Be("209912312359");
            message.Element("OP").Value.Should().Be("13");
            message.Element("MMSFILE").Value.Should().Be("VGVzdCB6aXAgZGF0YQ==");

            last_request_xml.Root.Element("MSGLST").Elements("MSG").Last().Element("MMSFILE").Should().BeNull();
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
            last_request_xml.FirstNode.NodeType.Should().Be(System.Xml.XmlNodeType.DocumentType);
            var type = last_request_xml.FirstNode as XDocumentType;
            type.Name.Should().Be("SESSION");
            type.SystemId.Should().Be("pswincom_submit.dtd");
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
            last_request_xml.Declaration.Encoding.Should().Be("iso-8859-1");
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

            last_request_xml.Descendants("MSG").First().Element("TARIFF").Value.Should().Be("100");
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

            msg1.NumInSession.Should().Be(1);
            msg2.NumInSession.Should().Be(2);
        }

        [Test]
        public void Should_return_status_on_messages_using_userref_if_possible()
        {
            var msg1 = new Sms { UserReference = "message1" };
            var msg2 = new Sms { UserReference = "message2" };
            var msg3 = new Sms { UserReference = "message3" };

            
            Transport_returns(
                message_result("2", "OK"), 
                message_result("1", "FAIL"),
                message_result("3", "INVALID_STATUS"));

            var response = client.Send(new[] {
                msg1,
                msg2,
                msg3
            });

            response.Results.Count().Should().Be(3);
            response.Results.First((m) => m.UserReference == "message1").Status.Should().Be(MessageStatus.Fail);
            response.Results.First((m) => m.UserReference == "message2").Status.Should().Be(MessageStatus.Ok);
            response.Results.First((m) => m.UserReference == "message3").Status.Should().Be(MessageStatus.Fail);
        }

        [Test]
        public void Should_handle_batch_status()
        {
            Transport_returns_batch_status("FAIL", "Username or password is incorrect");

            var response = client.Send(new Sms());
            response.Status.Should().Be(BatchStatus.Fail);
            response.StatusText.Should().Be("Username or password is incorrect");
        }

        [Test]
        public void Should_allow_sending_no_messages_for_authorization_purposes()
        {
            Transport_returns_batch_status("OK", "");
            var response = client.Send();
            response.Status.Should().Be(BatchStatus.Ok);
            response.StatusText.Should().Be("");
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
            result.Results.Count().Should().Be(100);
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
