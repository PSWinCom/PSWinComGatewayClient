using System;
using System.Collections.Generic;
using System.Text;

namespace PSWinCom.Gateway.Client
{
    public class GatewayResponse
    {
        public BatchStatus Status { get; set; }
        public string StatusText { get; set; }
        public IEnumerable<MessageResult> Results { get; set; }
    }

    public static class GatewayResponseExtensions
    {
        public static void Each(this IEnumerable<MessageResult> results, Action<MessageResult> action)
        {
            foreach (var result in results)
                action(result);
        }
    }
}