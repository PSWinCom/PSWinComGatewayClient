using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSWinCom.Gateway.Client
{
    public class HttpTransport : ITransport
    {
        public HttpTransport(Uri uri)
        {

        }

        public TransportResult Send(System.Xml.Linq.XDocument document)
        {
            throw new NotImplementedException();
        }
    }
}
