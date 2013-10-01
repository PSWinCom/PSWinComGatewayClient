using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Threading.Tasks;

namespace PSWinCom.Gateway.Client
{
    public class GatewayClient : GatewayClientBase, IAsyncGatewayClient
    {
        public GatewayClient(ITransport transport)
            :base(transport)
        {

        }

        public async Task<SendResult> SendAsync(IEnumerable<Message> messages)
        {
            var transport = Transport as IAsyncTransport;
            if (transport == null)
                throw new ApplicationException("Async operations is not supported by the transport");
            var transportResult = await transport.SendAsync(BuildPayload(messages));
            return GetSendResult(messages, transportResult);
        }
    }
}
