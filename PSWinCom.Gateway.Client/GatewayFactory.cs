using System;
using System.Collections.Generic;
using System.Linq;

namespace PSWinCom.Gateway.Client
{
    public static partial class Gateway
    {
        private static class Factory
        {
            private static string _defaultAddress = "http://sms3.pswin.com/sms";
            private static string _defaultUsername = "";
            private static string _defaultPassword = "";

            public static string DefaultAddress
            {
                get { return _defaultAddress; }
                set { _defaultAddress = value; }
            }

            public static string DefaultUsername
            {
                get { return _defaultUsername; }
                set { _defaultUsername = value; }
            }

            public static string DefaultPassword
            {
                get { return _defaultPassword; }
                set { _defaultPassword = value; }
            }

            public static IGatewayClient Client()
            {
                return Client(_defaultAddress).WithLogin(_defaultUsername, _defaultPassword);
            }

            public static IGatewayClient Client(string username, string password)
            {
                return Client().WithLogin(username, password);
            }

            public static IGatewayClient Client(string uri)
            {
                return Client(new Uri(uri)).WithLogin(_defaultUsername, _defaultPassword);
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

        }

        public static IGatewayClient WithLogin(this IGatewayClient client, string username, string password)
        {
            client.Username = username;
            client.Password = password;
            return client;
        }

        public static IGatewayClient Batched(this IGatewayClient client, int batchSize)
        {
            client.BatchSize = batchSize;
            return client;
        }
    }
}
