using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Collections;

namespace PSWinCom.Gateway.Client
{
    public class HttpTransport : ITransport
    {
        private Uri _uri;
        public HttpTransport(Uri uri)
        {
            _uri = uri;
        }

        public TransportResult Send(XDocument document)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_uri);

            request.ContentType = "application/xml";

            request.Method = "POST";

            var result = new TransportResult();

            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms, Encoding.GetEncoding("ISO-8859-1")))
                {
                    document.Save(sw);
                    ms.Seek(0, SeekOrigin.Begin);
                    var bytes = ms.ToArray();
                    request.ContentLength = bytes.Length + 1;

                    Stream requestStream = null;
                    try
                    {
                        requestStream = request.GetRequestStream();
                        requestStream.Write(bytes, 0, bytes.Length);
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
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    result.Success = (response.StatusCode == HttpStatusCode.OK);
                    using (var xr = XmlReader.Create(response.GetResponseStream()))
                    {
                        var content = XDocument.Load(xr);
                        result.Content = content;
                    }
                }
            }
            catch
            {
                result.Success = false;
            }

            return result;
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
