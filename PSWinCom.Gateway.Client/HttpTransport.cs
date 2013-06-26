using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PSWinCom.Gateway.Client
{
    public class HttpTransport : IAsyncTransport
    {
        private readonly Uri _uri;
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public HttpTransport(Uri uri)
        {
            _uri = uri;
        }

        public TransportResult Send(XDocument document)
        {
            return SendAsync(document).Result;
        }

        public async Task<TransportResult> SendAsync(XDocument document) {
            var client = new HttpClient();
            var ms = new MemoryStream();

            document.Save(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var content = new StreamContent(ms);

            content.Headers.Add("Content-Type", "application/xml");

            var request = client.PostAsync(_uri, content);
            var res = await request;

            var result = new TransportResult();
            result.Success = (res.StatusCode == HttpStatusCode.OK);
            var contentstring = await res.Content.ReadAsStringAsync();
            result.Content = XDocument.Parse(contentstring);

            return result;
        }
    }
}
