using System;
using System.Collections.Generic;
using System.Linq;

namespace PSWinCom.Gateway.Client
{
    public static partial class Gateway
    {
        public static string DefaultAddress
        {
            get { return Factory.DefaultAddress; }
            set { Factory.DefaultAddress = value; }
        }

        public static string DefaultUsername
        {
            get { return Factory.DefaultUsername; }
            set { Factory.DefaultUsername = value; }
        }

        public static string DefaultPassword
        {
            get { return Factory.DefaultPassword; }
            set { Factory.DefaultPassword = value; }
        }

        public static IAsyncGatewayClient Client()
        {
            return (IAsyncGatewayClient)Factory.Client();
        }

        public static IAsyncGatewayClient Client(string username, string password)
        {
            return (IAsyncGatewayClient)Factory.Client(username, password);
        }

        public static IAsyncGatewayClient Client(string uri)
        {
            return (IAsyncGatewayClient)Factory.Client(uri);
        }

        public static IAsyncGatewayClient Client(string uri, string username, string password)
        {
            return (IAsyncGatewayClient)Factory.Client(uri, username, password);
        }

        public static IAsyncGatewayClient Client(Uri uri, string username, string password)
        {
            return (IAsyncGatewayClient)Factory.Client(uri, username, password);
        }

        public static IAsyncGatewayClient Client(Uri uri)
        {
            return (IAsyncGatewayClient)Factory.Client(uri);
        }
    }
}
