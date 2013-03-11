using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Should;
using Moq;
using System.Xml.Linq;

namespace PSWinCom.Gateway.Client.Tests
{
    [TestFixture]
    public class MessageTests
    {
        [Test]
        public void Should_default_to_num_in_session_if_myreference_is_not_set()
        {
            var msg = new Message();
            msg.NumInSession = 3;
            msg.UserReference.ShouldEqual("3");
        }

        [Test]
        public void Should_return_myreference_if_set()
        {
            var msg = new Message();
            msg.NumInSession = 3;
            msg.UserReference = "something";
            msg.UserReference.ShouldEqual("something");
            
        }
    }
}