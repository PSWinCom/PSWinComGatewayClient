using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PSWinCom.Gateway.Receiver;
using System.Xml.Linq;
using Should;

namespace PSWinCom.Gateway.Client.Receiver.Tests
{
    [TestFixture]
    public class IncomingHandlerTests
    {
        [Test]
        public void Should_parse_and_handle_messages()
        {
            var handler = new IncomingMessageHandlerStub();

            handler.Handle(
                new XDocument(
                    new XElement("MSGLST",
                        new XElement("MSG",
                            new XElement("ID", "1"),
                            new XElement("TEXT", "Some message"),
                            new XElement("SND", "12345678"),
                            new XElement("RCV", "26112")
                        )
                    )
                )
            );

            handler.LastMessage.From.ShouldEqual("12345678");
            handler.LastMessage.To.ShouldEqual("26112");
            handler.LastMessage.Text.ShouldEqual("Some message");
        }
    }
}
