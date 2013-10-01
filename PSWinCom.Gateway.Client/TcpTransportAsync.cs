using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSWinCom.Gateway.Client
{
    public partial class TcpTransport: IAsyncTransport
    {
        public async Task<TransportResult> SendAsync(System.Xml.Linq.XDocument document)
        {
            return Send(document);
        }
    }
}
