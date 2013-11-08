using System;
using System.Collections.Generic;
using System.Linq;

namespace PSWinCom.Gateway.Client
{
    public static class ClientFactory
    {
        public static IGatewayClient GetClient()
        {
            return GetClient("https://sms3.pswin.com/sms");
        }

        public static IGatewayClient GetClient(string uri)
        {
            return GetClient("https://sms3.pswin.com/sms");
        }

        public static IGatewayClient GetClient(string username, string password)
        {
            return GetClient("https://sms3.pswin.com/sms", username, password);
        }

        public static IGatewayClient GetClient(string uri, string username, string password)
        {
            return GetClient(new Uri(uri), username, password);
        }

        public static IGatewayClient GetClient(Uri uri, string username, string password)
        {
            var client = GetClient(uri);
            client.Username = username;
            client.Password = password;
            return client;
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
