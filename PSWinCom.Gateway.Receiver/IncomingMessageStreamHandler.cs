using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace PSWinCom.Gateway.Receiver
{
    public abstract class IncomingMessageStreamHandler : IncomingMessageHandler
    {
        Encoding latin1 = Encoding.GetEncoding("ISO-8859-1");
        public Encoding Encoding { get { return latin1; } }

        public void HandleRequest(System.IO.Stream inStream, System.IO.Stream outStream)
        {
            XDocument request = (XDocument)XDocument.ReadFrom(XmlReader.Create(inStream));

            var result = HandleRequest(request);

            result.WriteTo(
                XmlWriter.Create(
                    outStream,
                    new XmlWriterSettings
                    {
                        Encoding = Encoding,
                        Indent = false,
                    }
                )
            );
        }
    }
}
