using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace PSWinCom.Gateway.Client
{
    public interface ITransport
    {
        TransportResult Send(XDocument document);
    }
}
