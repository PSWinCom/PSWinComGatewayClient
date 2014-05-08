using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PSWinCom.Gateway.Client
{
    public partial class HttpTransport : IAsyncTransport
    {
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public async Task<TransportResult> SendAsync(XDocument document)
        {

            var request = (HttpWebRequest)WebRequest.Create(_uri);
            request.ContentType = "application/xml";
            request.Method = "POST";
            var result = new TransportResult();

            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms, Encoding.GetEncoding("ISO-8859-1")))
                {
                    document.Save(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    var bytes = ms.ToArray();
                    request.ContentLength = bytes.Length + 1;

                    Stream requestStream = null;
                    try
                    {
                        requestStream = await request.GetRequestStreamAsync();
                        await requestStream.WriteAsync(bytes, 0, bytes.Length);
                        requestStream.WriteByte(0x00);
                    }
                    finally
                    {
                        if (requestStream != null)
                        {
                            requestStream.Close();
                        }
                    }
                }
            }

            try
            {
                using (var response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    result.Success = (response.StatusCode == HttpStatusCode.OK);
                    if (result.Success)
                    {
                        using (var responseStream = response.GetResponseStream())
                        using (var sr = new StreamReader(responseStream, Encoding.GetEncoding("ISO-8859-1")))
                        {
                            var contentstring = await sr.ReadToEndAsync();
                            result.Content = XDocument.Parse(contentstring);
                        }
                    }
                }
            }
            catch
            {
                result.Success = false;
            }

            return result;
        }

    }
}
