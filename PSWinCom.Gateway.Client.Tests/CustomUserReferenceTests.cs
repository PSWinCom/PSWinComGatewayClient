using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Should;

namespace PSWinCom.Gateway.Client.Tests
{
    [TestFixture]
    public class CustomUserReferenceTests
    {
        [Test]
        public void Should_default_to_num_in_session_if_myreference_is_not_set()
        {
            var msg = new SmsMessage();
            msg.NumInSession = 3;
            msg.UserReference.ShouldEqual("3");
        }

        [Test]
        public void Should_return_myreference_if_set()
        {
            var msg = new SmsMessage();
            msg.NumInSession = 3;
            msg.UserReference = "something";
            msg.UserReference.ShouldEqual("something");
        }
    }
}
