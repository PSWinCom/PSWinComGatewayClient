using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Should;

namespace PSWinCom.Gateway.Client.Tests
{
    [TestFixture]
    public class FrameworkSpecificFactoryTests
    {
        [Test]
        public void Should_return_async_client()
        {
            var client = Gateway.Client();
            client.ShouldImplement<IAsyncGatewayClient>();
        }
    }
}
