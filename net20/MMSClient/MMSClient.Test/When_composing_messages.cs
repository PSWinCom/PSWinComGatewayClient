using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PSWinCom.Gateway.MMS.Client;
using System.IO;

namespace MMSClient.Test
{
    [TestFixture]
    public class When_composing_messages
    {
    	[Test]
        public void Can_save_to_stream_and_load_from_stream()
        {
            var message = new MMSMessage();
            var part = new MMSMessagePart { Name = "picture.jpg", PartId = 1 };
            part.Load("testfiles/picture.jpg");

            message.Parts.Add(1, part);

            using (var stream = new MemoryStream())
            {
                message.SaveCompressed(stream);
                Assert.IsTrue(stream.Length > 0);

                var msg2 = new MMSMessage();
                stream.Seek(0, SeekOrigin.Begin);

                msg2.LoadCompressed(stream);
                Assert.AreEqual(1, msg2.Parts.Count);
                msg2.Parts[0].Name = "picture.jpg";
            }
        }
    }

    [TestFixture]
    public class When_decomposing_mms
    {
        [Test]
        public void Test()
        {
            
            var m = new PSWinCom.Gateway.MMS.Client.MMSMessage();
            m.LoadCompressed("testfiles/unziptest.zip");
            foreach (int n in m.Parts.Keys)
            {
                MMSMessagePart p = m.Parts[n];
                p.Save(@"c:\PSWinCom\WebApps\CustomerServices\SMS2Email\Tmp\");
            }
        }
    }
}
