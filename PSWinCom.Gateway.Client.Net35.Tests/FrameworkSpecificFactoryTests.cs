using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Should;

namespace PSWinCom.Gateway.Client.Tests
{
    [TestFixture]
    public class FrameworkSpecificFactoryTests
    {
        [Test]
        public void Should_return_async_client()
        {
            Gateway.DefaultUsername = "";
            Gateway.DefaultPassword = "";
            var client = Gateway.Client();
            client.ShouldImplement<IGatewayClient>();
            client.Username.ShouldEqual("");
            client.Password.ShouldEqual("");
        }
    }
}
