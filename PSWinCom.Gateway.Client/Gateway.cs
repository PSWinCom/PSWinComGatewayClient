using System;
using System.Collections.Generic;
using System.Linq;

namespace PSWinCom.Gateway.Client
{
    public static class Gateway
    {
        private static string _defaultGateway = "http://sms3.pswin.com/sms";
        public static string DefaultAddress
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

        public static IGatewayClient Client()
        {
            return Client(_defaultGateway);
        }

        public static IGatewayClient Client(string username, string password)
        {
            return Client().WithLogin(username, password);
        }

        public static IGatewayClient Client(string uri)
        {
            return Client(new Uri(uri));
        }

        public static IGatewayClient Client(string uri, string username, string password)
        {
            return Client(uri).WithLogin(username, password);
        }

        public static IGatewayClient Client(Uri uri, string username, string password)
        {
            return Client(uri).WithLogin(username, password);
        }

        public static IGatewayClient Client(Uri uri)
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
