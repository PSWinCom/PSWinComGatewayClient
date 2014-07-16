using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections;

namespace PSWinCom.Gateway.Client.MessageFormatters
{
    public class WapPush
    {
        private string _title;
        private Uri _url;
        protected static byte[] WDP_DESTINATIONPORT = new byte[] { 0x0b, 0x84 };
        protected static byte[] WDP_SOURCEPORT = new byte[] { 0x23, 0xf0 };
        ServiceIndication serviceIndication;

        private byte[] pdubytes;

        public WapPush(string title, string url)
            : this(title, new Uri(url))
        {
        }

        public WapPush(string title, Uri url)
        {
            serviceIndication = new ServiceIndication(url.ToString(), title, ServiceIndicationAction.SignalHigh);

            Title = title;
            Url = url;
        }

        public Uri Url
        {
            get
            {
                return _url;
            }
            set
            {
                _url = value;
                serviceIndication.Href = _url.ToString();
            }
        }

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                serviceIndication.Href = _title;
            }
        }

        private static string Compile(WapPush pushmessage)
        {
            return String.Join("", pushmessage.GetPDUBytes().Select(b => b.ToString("X2")).ToArray());
        }

        public override string ToString()
        {
            return Compile(this);
        }

        public static implicit operator string(WapPush pushmessage)
        {
            return Compile(pushmessage);
        }

        public static implicit operator byte[](WapPush pushmessage)
        {
            return pushmessage.GetPDUBytes();
        }

        /// <summary>
        /// Generates the PDU (Protocol Data Unit) comprising the encoded Service Indication
        /// and the WSP (Wireless Session Protocol) headers
        /// </summary>
        /// <returns>byte array comprising the PDU</returns>
        private byte[] GetPDUBytes()
        {
            byte[] body = serviceIndication.GetWBXMLBytes();

            byte[] headerBuffer = GetWSPHeaderBytes((byte)body.Length);

            MemoryStream stream = new MemoryStream();
            stream.Write(headerBuffer, 0, headerBuffer.Length);
            stream.Write(body, 0, body.Length);

            return stream.ToArray();
        }

        /// <summary>
        /// Generates the WSP (Wireless Session Protocol) headers with the well known
        /// byte values specfic to a Service Indication
        /// </summary>
        /// <param name="contentLength">the length of the encoded Service Indication</param>
        /// <returns>byte array comprising the headers</returns>
        private byte[] GetWSPHeaderBytes(byte contentLength)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                stream.WriteByte(WSP.TRANSACTIONID_CONNECTIONLESSWSP);
                stream.WriteByte(WSP.PDUTYPE_PUSH);
                using (MemoryStream headersStream = new MemoryStream())
                {
                    headersStream.Write(WSP.HEADER_CONTENTTYPE_application_vnd_wap_sic_utf_8, 0, WSP.HEADER_CONTENTTYPE_application_vnd_wap_sic_utf_8.Length);
                    headersStream.WriteByte(WSP.HEADER_APPLICATIONTYPE);
                    headersStream.WriteByte(WSP.HEADER_APPLICATIONTYPE_x_wap_application_id_w2);
                    headersStream.Write(WSP.HEADER_PUSHFLAG, 0, WSP.HEADER_PUSHFLAG.Length);
                    stream.WriteByte((byte)headersStream.Length);
                    headersStream.WriteTo(stream);
                }
                return stream.ToArray();
            }
        }

    }
}
