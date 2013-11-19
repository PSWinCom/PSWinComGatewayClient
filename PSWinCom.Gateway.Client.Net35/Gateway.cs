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

        public static IGatewayClient Client()
        {
            return Factory.Client();
        }

        public static IGatewayClient Client(string username, string password)
        {
            return Factory.Client(username, password);
        }

        public static IGatewayClient Client(string uri)
        {
            return Factory.Client(uri);
        }

        public static IGatewayClient Client(string uri, string username, string password)
        {
            return Factory.Client(uri, username, password);
        }

        public static IGatewayClient Client(Uri uri, string username, string password)
        {
            return Factory.Client(uri, username, password);
        }

        public static IGatewayClient Client(Uri uri)
        {
            return Factory.Client(uri);
        }
    }
}
