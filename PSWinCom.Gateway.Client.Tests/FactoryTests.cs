using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;

namespace PSWinCom.Gateway.Client.Tests
{
    [TestFixture]
    public class FactoryTests
    {
        [Test]
        public void Should_consider_defaults()
        {
            Gateway.DefaultAddress = "http://my-private-gw.pswin.com/";
            Gateway.DefaultUsername = "myusername";
            Gateway.DefaultPassword = "mypassword";

            var client = Gateway.Client();

            client.Username.Should().Be("myusername");
            client.Password.Should().Be("mypassword");
            client.Transport.Uri.Should().Be(new Uri("http://my-private-gw.pswin.com/"));
        }

        [Test]
        public void Should_load_proper_protocol()
        {
            Gateway.Client("http://my-private-gw.pswin.com/").Transport.Should().BeOfType<HttpTransport>();
            Gateway.Client("tcp://1.1.1.1:1234").Transport.Should().BeOfType<TcpTransport>();
        }

        [SetUp]
        public void Setup()
        {
            Gateway.DefaultUsername = "";
            Gateway.DefaultPassword = "";
        }

        [Test]
        public void Fluid_credentials()
        {
            var fluidclient = Gateway.Client().WithLogin("test", "password");
            fluidclient.Username.Should().Be("test");
            fluidclient.Password.Should().Be("password");
        }

        [Test]
        public void Fluid_batch_size()
        {
            var fluidclient = Gateway.Client().Batched(100);
            fluidclient.BatchSize.Should().Be(100);
        }

    }

}
