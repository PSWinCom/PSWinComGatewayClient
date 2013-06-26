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
    public class HttpTransport : ITransport
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
            var content = new StreamContent(ms);

            content.Headers.Add("Content-Type", "text/xml");

            var request = client.PostAsync(_uri, content);
            var res = request.Result;

            var result = new TransportResult();
            result.Success = (res.StatusCode == HttpStatusCode.OK);
            result.Content = XDocument.Parse(res.Content.ReadAsStringAsync().Result);

            return result;
        }
    }
}
