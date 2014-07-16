using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Should;

namespace PSWinCom.Gateway.Client.MessageFormatters.Tests
{
    [TestFixture]
    public class WapPushMessageTests
    {
        [Test]
        public void Should_include_correct_header()
        {
            byte[] wapbytes = new WapPush("PSWinCom", "https://www.pswin.com");
            wapbytes.Take(11).ShouldEqual(new byte[] { 0x25, 0x06, 0x08, 0x03, 0xAE, 0x81, 0xEA, 0xAF, 0x82, 0xB4, 0x84 });
        }

        [Test]
        public void Should_build_proper_body_start()
        {
            byte[] wapbytes = new WapPush("A", "http://j.mp/pswin");
            wapbytes.Skip(11).Take(4).ShouldEqual(new byte[] { 0x01, 0x05, 0x6A, 0x00 });
            wapbytes.Skip(15).Take(2).ShouldEqual(new byte[] { 0x5 | 0x40, 0x6 | 0xC0 | 0x40 });
        }

        [TestCase("http://j.mp/pswin", 0xC, "j.mp/pswin")]
        [TestCase("http://www.pswin.com", 0xD, "pswin.com")]
        [TestCase("https://j.mp/pswin", 0xE, "j.mp/pswin")]
        [TestCase("https://www.pswin.com/", 0xF, "pswin.com/")]
        [TestCase("mailto:support@pswin.com", 0xB, "mailto:support@pswin.com")]
        public void Should_shorten_url_if_possible(string url, byte expectedStartToken, string expectedContent)
        {
            byte[] wapbytes = new WapPush("A", url);
            wapbytes.Skip(17).First().ShouldEqual(expectedStartToken);
            wapbytes.Skip(18).First().ShouldEqual((byte)0x03);
            var expectedContentBytes = Encoding.UTF8.GetBytes(expectedContent);
            wapbytes.Skip(19).Take(expectedContentBytes.Length).ToArray().ShouldEqual(expectedContentBytes);
        }

        [TestCase("PSWinCom AS")]
        [TestCase("Ærlig SMS leverandør")]
        public void Should_include_text(string text)
        {
            byte[] wapbytes = new WapPush(text, "http://www.pswin.com");
            var expectedContentBytes = Encoding.UTF8.GetBytes(text);
            wapbytes[30].ShouldEqual((byte)0x08);
            wapbytes[31].ShouldEqual((byte)0x01);
            wapbytes[32].ShouldEqual((byte)0x03);
            wapbytes.Skip(33).Take(expectedContentBytes.Length).ToArray().ShouldEqual(expectedContentBytes);
            wapbytes.Skip(33 + expectedContentBytes.Length).Take(3).ToArray().ShouldEqual(new byte[] { 0x00, 0x01, 0x01 });
        }
    }
}
