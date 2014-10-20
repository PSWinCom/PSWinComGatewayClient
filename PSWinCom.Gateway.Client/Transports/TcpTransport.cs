using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace PSWinCom.Gateway.Client
{
    public partial class TcpTransport : ITransport
    {
        private Uri _uri;

        public TcpTransport()
            :this(new Uri("tcp://gw2-fro.pswin.com:1111"))
        {

        }

        public TcpTransport(string host, int port)
            : this(new Uri(String.Format("tcp://{0}:{1}", host, port)))
        {

        }

        public TcpTransport(Uri uri)
        {
            if (uri.Scheme.ToLower() != "tcp")
                throw new ArgumentException("Uri scheme must be tcp!");
            _uri = uri;
        }

        public TransportResult Send(XDocument document)
        {
            var client = new TcpClient(_uri.Host, _uri.Port);
            using (var sendData = new MemoryStream())
            {
                using (var xw = XmlWriter.Create(sendData, new XmlWriterSettings() { Encoding = Encoding.GetEncoding("ISO-8859-1") }))
                {
                    document.Save(xw);
                }

                var networkStream = client.GetStream();
                networkStream.Write(sendData.ToArray(), 0, (int)sendData.Length);
                networkStream.WriteByte(0);

                using (var receivedData = new MemoryStream())
                {
                    var buffer = new byte[256];

                    int read = 0;
                    do
                    {
                        read = networkStream.Read(buffer, 0, buffer.Length);

                        if (read > 0)
                        {
                            // Check if last byte is a zero byte, if so, don't include in receivedData
                            if (buffer[read - 1] == 0) read--;
                            receivedData.Write(buffer, 0, read);
                        }
                    }
                    while (read == buffer.Length);

                    var result = new TransportResult();
                    try
                    {
                        receivedData.Seek(0, SeekOrigin.Begin);
                        var xr = XmlReader.Create(receivedData);
                        result.Content = XDocument.Load(xr);
                        result.Success = true;
                    }
                    catch
                    {
                        result.Success = false;
                    }

                    return result;
                }
            }
        }

        public Uri Uri
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
