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
        private Uri _uri;
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

            var res = await PostRequest(document);

            var result = new TransportResult()
            {
                Success = res.IsSuccessStatusCode,
            };

            if (res.IsSuccessStatusCode)
            {
                var contentstring = await res.Content.ReadAsStringAsync();
#if (DEBUG)
                Console.Write(contentstring);
#endif
                result.Content = XDocument.Parse(contentstring);
            }

            return result;
        }

        private async Task<HttpResponseMessage> PostRequest(XDocument document)
        {
            var client = new HttpClient();
            var ms = new MemoryStream();

            document.Save(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var content = new StreamContent(ms);

            content.Headers.Add("Content-Type", "application/xml");

            return await client.PostAsync(_uri, content);
        }

        public Uri Uri
        {
            get
            {
                return _uri;
            }
            set
            {
                _uri = value;
            }
        }
    }
}
