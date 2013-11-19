using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace PSWinCom.Gateway.Client
{
    public interface ITransport
    {
        Uri Uri { get; set; }
        TransportResult Send(XDocument document);
    }
}
