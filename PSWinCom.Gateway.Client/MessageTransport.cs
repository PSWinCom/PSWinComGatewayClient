using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace PSWinCom.Gateway.Client
{
    public abstract class Transport
    {
        public abstract SendResult Send(XDocument document);
    }
}
