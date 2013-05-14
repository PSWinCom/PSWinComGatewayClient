using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace PSWinCom.Gateway.Client
{
    public abstract class ITransport
    {
        public abstract TransportResult Send(XDocument document);
    }
}
