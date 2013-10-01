using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PSWinCom.Gateway.Client
{
    public class GatewayClient : GatewayClientBase
    {
        public GatewayClient(ITransport transport)
            :base(transport)
        {

        }

    }
}
