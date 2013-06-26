using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PSWinCom.Gateway.Client
{
    public interface IAsyncTransport: ITransport
    {
        Task<TransportResult> SendAsync(XDocument document);
    }
}
