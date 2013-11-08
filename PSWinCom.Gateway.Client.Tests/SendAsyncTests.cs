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
                var result = client.SendAsync(new[] { new SmsMessage { Text = "Test", ReceiverNumber = "4790871951" } }).Result;
            }
            catch (AggregateException ex)
            {
                ex.InnerExceptions.Where(e => e is ApplicationException).Count().ShouldEqual(1);
            }
        }
    }
}
