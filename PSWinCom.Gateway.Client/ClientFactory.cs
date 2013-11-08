using System;
using System.Collections.Generic;
using System.Linq;

namespace PSWinCom.Gateway.Client
{
    public static class Gateway
    {
        private static string _defaultGateway = "https://sms3.pswin.com/sms";
        public static string DefaultGateway
        {
            get
            {
                return _defaultGateway;
            }
            set
            {
                _defaultGateway = value;
            }
        }

        public static IGatewayClient GetClient()
        {
            return GetClient(_defaultGateway);
        }

        public static IGatewayClient GetClient(string username, string password)
        {
            return GetClient().WithLogin(username, password);
        }

        public static IGatewayClient GetClient(string uri)
        {
            return GetClient(new Uri(uri));
        }

        public static IGatewayClient GetClient(string uri, string username, string password)
        {
            return GetClient(uri).WithLogin(username, password);
        }

        public static IGatewayClient GetClient(Uri uri, string username, string password)
        {
            return GetClient(uri).WithLogin(username, password);
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

        public static IGatewayClient WithLogin(this IGatewayClient client, string username, string password)
        {
            client.Username = username;
            client.Password = password;
            return client;
        }
    }
}
