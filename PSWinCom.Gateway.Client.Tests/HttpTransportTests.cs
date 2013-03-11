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
        [Test]
        public void Should_pass_on_xml()
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://*:56123/listener/");
            listener.Start();

            var transport = new HttpTransport(new Uri("http://localhost:56123/listener/"));

            XDocument document = new XDocument(new XElement("TEST"));
            transport.Send(document);

            var context = listener.GetContext();

            var request_body = new StreamReader(context.Request.InputStream).ReadToEnd();

            request_body.ShouldEqual(expected_string_representation_of(document));

            listener.Stop();
        }

        private string expected_string_representation_of(XDocument document) {
            using (var expectedStream = new MemoryStream())
            {
                document.Save(expectedStream);
                expectedStream.Seek(0, SeekOrigin.Begin);
                return new StreamReader(expectedStream).ReadToEnd();
            }
        }
    }
}
