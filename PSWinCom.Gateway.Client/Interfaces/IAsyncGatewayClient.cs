using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PSWinCom.Gateway.Client
{
    public interface IAsyncGatewayClient : IGatewayClient
    {
        Task<GatewayResponse> SendAsync(IEnumerable<Message> messages);
        Task<GatewayResponse> SendAsync(string sessionData, IEnumerable<Message> messages);
    }
}
