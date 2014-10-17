﻿using System;
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

        public GatewayClient(ITransport transport, string username, string password)
            : base(transport)
        {
            Username = username;
            Password = password;
        }


        public async Task<GatewayResponse> SendAsync(IEnumerable<Message> messages)
        {
            var transport = Transport as IAsyncTransport;
            if (transport == null)
                throw new ApplicationException("Async operations is not supported by the transport");
            var transportResult = await transport.SendAsync(BuildPayload(messages));
            var result = new GatewayResponse();
            result.Results = ParseTransportResults(messages, transportResult);
            return result;
        }
    }
}