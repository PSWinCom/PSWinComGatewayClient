using System;
using System.Collections.Generic;
using System.Linq;

namespace PSWinCom.Gateway.Client
{
    public static class ClientFactory
    {
        public static IGatewayClient GetHttpClient(string url)
        {
            return new GatewayClient(new HttpTransport(new Uri(url)));
        }

        public static IGatewayClient GetHttpClient()
        {
            return GetHttpClient("https://sms3.pswin.com/sms");
        }

        public static IGatewayClient GetTcpClient()
        {
            return new GatewayClient(new TcpTransport());
        }

        public static IGatewayClient GetClient(string uri)
        {
            return GetClient(new Uri(uri));
        }

        public static IGatewayClient GetClient(Uri uri)
        {
            switch (uri.Scheme)
            {
                case "tcp":
                    return new GatewayClient(new TcpTransport(uri));
                case "http":
                case "https":
                    return new GatewayClient(new HttpTransport(uri));
                default:
                    throw new ArgumentException("Uri scheme is not supported");
            }
        }
    }
}
