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
using System.IO;

namespace PSWinCom.Gateway.Client.Tests
{
    [TestFixture]
    public class HttpTransportTests
    {
        private HttpTransport transport;

        [Test]
        public void Should_pass_on_xml()
        {
            XDocument document = new XDocument(new XElement("TEST"));

            with_listener(
                at: "http://localhost:56123/listener/error/",
                that_returns: 500,
                with_body: new XDocument(document),
                ensure: (transport, request) =>
                {
                    transport
                        .Send(document)
                        .Success
                        .ShouldBeFalse();
                    request
                        .Result
                        .ShouldEqual(expected_string_representation_of(document));
                }
            );
        }

        [Test]
        public void Should_return_success_when_status_code_is_200()
        {
            with_listener(at: "http://localhost:56123/listener/ok/",
                that_returns: 200,
                with_body: new XDocument(new XElement("ROOT")),
                ensure: (transport, server) =>
                {
                    transport
                        .Send(new XDocument(new XElement("TEST")))
                        .Success
                        .ShouldBeTrue();
                }
            );
        }

        private static void with_listener(string at, int that_returns, XDocument with_body, Action<HttpTransport, Task<string>> ensure) {

            var endpoint = new Uri(at);

            Task<string> server = listener(
                    at: at,
                    that_returns: that_returns,
                    with_body: with_body);
            server.Start();

            var transport = new HttpTransport(endpoint);
            ensure(transport, server);
        }

        [SetUp]
        public void Setup()
        {
            transport = new HttpTransport(new Uri("http://localhost:56123/listener/"));
        }

        private static Task<string> listener(string at, int that_returns, XDocument with_body)
        {
            var server = new Task<string>((s) =>
            {
                var state = (Tuple<int, XDocument>)s;

                var listener = new HttpListener();
                listener.Prefixes.Add(at);
                listener.Start();

                var context = listener.GetContext();

                var request_body = new StreamReader(context.Request.InputStream).ReadToEnd();

                context.Response.StatusCode = state.Item1;
                state.Item2.Save(context.Response.OutputStream);

                context.Response.Close();

                return request_body;
            }, new Tuple<int, XDocument>(that_returns, with_body));
            return server;
        }
        private string expected_string_representation_of(XDocument document)
        {
            using (var expectedStream = new MemoryStream())
            {
                document.Save(expectedStream);
                expectedStream.Seek(0, SeekOrigin.Begin);
                return new StreamReader(expectedStream).ReadToEnd();
            }
        }
    }
}
