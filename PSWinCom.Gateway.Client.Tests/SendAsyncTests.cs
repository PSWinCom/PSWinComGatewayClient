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
    public class SendAsyncTests
    {
        [Test]
        public void Should_throw_exception_when_trying_to_call_async_send_with_no_async_transport()
        {
            var mockTransport = new Mock<ITransport>();

            var client = new PSWinCom.Gateway.Client.GatewayClient(mockTransport.Object);
            try
            {
                var result = client.SendAsync(new[] { new Sms { Text = "Test", ReceiverNumber = "4790871951" } }).Result;
            }
            catch (AggregateException ex)
            {
                ex.InnerExceptions.Where(e => e is ApplicationException).Count().ShouldEqual(1);
            }
        }

        [Test]
        public async void Should_be_able_to_send_with_async_transport()
        {
            var client = new PSWinCom.Gateway.Client.GatewayClient(mockAsyncTransport.Object);

            Async_transport_returns(message_result("1", "OK"));

            var response = await client.SendAsync(new[] { 
                new Sms { 
                    Text = "Test", 
                    ReceiverNumber = "4790871951" 
                } 
            });

            response.Results.First().UserReference.ShouldEqual("1");
            response.Results.First().Status.ShouldEqual("OK");
        }

        [SetUp]
        public void Setup()
        {
            mockAsyncTransport = new Mock<IAsyncTransport>();
        }

        private void Async_transport_returns(params XElement[] results)
        {
            mockAsyncTransport
                .Setup((t) => t.SendAsync(It.IsAny<XDocument>()))
                .Returns(Task.FromResult<TransportResult>(new TransportResult
                {
                    Content = new XDocument(
                        new XElement("SESSION",
                            new XElement("MSGLST",
                                results
                            )
                        )
                    )
                }));
        }

        private static XElement message_result(string numInSession, string status)
        {
            return new XElement("MSG",
                new XElement("ID", numInSession),
                new XElement("STATUS", status)
            );
        }

        private Mock<IAsyncTransport> mockAsyncTransport;
    }
}
