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
    public abstract class IncomingMessageHttpHandler : IncomingMessageStreamHandler, IHttpHandler
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/xml";
            context.Response.ContentEncoding = Encoding;
            HandleRequest(context.Request.InputStream, context.Response.OutputStream);
            context.Response.Flush();
            context.Response.End();
        }

    }
}