using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace PSWinCom.Gateway.Client
{
    public class HttpTransport : Transport
    {
        private readonly Uri _uri;
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public HttpTransport(Uri uri)
        {
            _uri = uri;
        }

        public override TransportResult Send(XDocument document)
        {
            var client = new HttpClient();

            var ms = new MemoryStream();

            document.Save(ms);
            ms.Seek(0, SeekOrigin.Begin);

            client.PostAsync(_uri, new StreamContent(ms));

            return null;
        }
    }
}
