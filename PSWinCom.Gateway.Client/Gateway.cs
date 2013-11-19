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

        public static IAsyncGatewayClient Client()
        {
            return (IAsyncGatewayClient)GatewayFactory.Client();
        }

        public static IAsyncGatewayClient Client(string username, string password)
        {
            return (IAsyncGatewayClient)GatewayFactory.Client(username, password);
        }

        public static IAsyncGatewayClient Client(string uri)
        {
            return (IAsyncGatewayClient)GatewayFactory.Client(uri);
        }

        public static IAsyncGatewayClient Client(string uri, string username, string password)
        {
            return (IAsyncGatewayClient)GatewayFactory.Client(uri, username, password);
        }

        public static IAsyncGatewayClient Client(Uri uri, string username, string password)
        {
            return (IAsyncGatewayClient)GatewayFactory.Client(uri, username, password);
        }

        public static IAsyncGatewayClient Client(Uri uri)
        {
            return (IAsyncGatewayClient)GatewayFactory.Client(uri);
        }
    }
}
