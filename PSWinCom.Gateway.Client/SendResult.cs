using System;
using System.Collections.Generic;
using System.Text;

namespace PSWinCom.Gateway.Client
{
    public class SendResult
    {
        public IEnumerable<MessageResult> Results { get; set; }
    }

    public static class SendResultExtensions
    {
        public static void Each(this IEnumerable<MessageResult> results, Action<MessageResult> action)
        {
            foreach (var result in results)
                action(result);
        }
    }
}