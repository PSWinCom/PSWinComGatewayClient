using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Should;

namespace PSWinCom.Gateway.Client.Tests
{
    [TestFixture]
    public class FactoryTests
    {
        [Test]
        public void Should_consider_defaults()
        {
            Gateway.DefaultAddress = "http://my.stupid-doma.in/sms";
            Gateway.DefaultUsername = "yallaballa";
            Gateway.DefaultPassword = "ralla";

            var client = Gateway.Client();

            client.Username.ShouldEqual("yallaballa");
            client.Password.ShouldEqual("ralla");
            client.Transport.Uri.ShouldEqual(new Uri("http://my.stupid-doma.in/sms"));
        }

        [Test]
        public void Should_load_proper_protocol()
        {
            Gateway.Client("http://my.stupid-doma.in/sms").Transport.ShouldBeType<HttpTransport>();
            Gateway.Client("tcp://1.1.1.1:1234").Transport.ShouldBeType<TcpTransport>();
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
            fluidclient.Username.ShouldEqual("test");
            fluidclient.Password.ShouldEqual("password");
        }

        [Test]
        public void Fluid_batch_size()
        {
            var fluidclient = Gateway.Client().Batched(100);
            fluidclient.BatchSize.ShouldEqual(100);
        }

    }

}
