using System;
using System.Collections.Generic;
using System.Linq;

namespace PSWinCom.Gateway.Client
{
    public static partial class Gateway
    {
        public static string DefaultAddress
        {
            get { return GatewayFactory.DefaultAddress; }
            set { GatewayFactory.DefaultAddress = value; }
        }

        public static string DefaultUsername
        {
            get { return GatewayFactory.DefaultUsername; }
            set { GatewayFactory.DefaultUsername = value; }
        }

        public static string DefaultPassword
        {
            get { return GatewayFactory.DefaultPassword; }
            set { GatewayFactory.DefaultPassword = value; }
        }

        public static IGatewayClient Client()
        {
            return GatewayFactory.Client();
        }

        public static IGatewayClient Client(string username, string password)
        {
            return GatewayFactory.Client(username, password);
        }

        public static IGatewayClient Client(string uri)
        {
            return GatewayFactory.Client(uri);
        }

        public static IGatewayClient Client(string uri, string username, string password)
        {
            return GatewayFactory.Client(uri, username, password);
        }

        public static IGatewayClient Client(Uri uri, string username, string password)
        {
            return GatewayFactory.Client(uri, username, password);
        }

        public static IGatewayClient Client(Uri uri)
        {
            return GatewayFactory.Client(uri);
        }
    }
}
