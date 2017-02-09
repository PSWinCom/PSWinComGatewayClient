using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PSWinCom.Gateway.Client
{
    public abstract class GatewayClientBase : IGatewayClient
    {
        public ITransport Transport { get; set; }

        public GatewayClientBase(ITransport transport)
        {
            Transport = transport;
        }

        public virtual GatewayResponse Send(params Message[] messages)
        {
            return Send(String.Empty, messages);
        }

        public virtual GatewayResponse Send(string sessionData, params Message[] messages)
        {
            return Send(sessionData, messages.AsEnumerable());
        }

        public virtual GatewayResponse Send(IEnumerable<Message> messages)
        {
            return Send(String.Empty, messages);
        }

        public virtual GatewayResponse Send(string sessionData, IEnumerable<Message> messages)
        {
            var result = new GatewayResponse();


            if (messages.Count() == 0)
            {
                var transportResult = Transport.Send(BuildPayload(sessionData, messages));
                var authResult = GetSendResult(messages, transportResult);
                result.Status = authResult.Status;
                result.StatusText = authResult.StatusText;
            }
            else
            {
                var thisBatchSize = BatchSize > 0 ? BatchSize : messages.Count();
                var skip = 0;
                while (messages.Count() > skip)
                {
                    var messageList = messages.Skip(skip).Take(thisBatchSize).ToList();
                    var transportResult = Transport.Send(BuildPayload(sessionData, messageList));
                    var batchResult = GetSendResult(messageList, transportResult);

                    if (batchResult.Status == BatchStatus.Ok)
                        result.Results = result.Results == null ? batchResult.Results : result.Results.Union(batchResult.Results);
                    else
                    {
                        result.Status = batchResult.Status;
                        result.StatusText = batchResult.StatusText;
                    }

                    skip += thisBatchSize;
                }

            }

            return result;
        }

        protected XDocument BuildPayload(string sessionData, IEnumerable<Message> messages)
        {
            return 
                new XDocument(
                    new XDeclaration("1.0", "iso-8859-1", null),
                    new XElement("SESSION",
                        new XElement("CLIENT", Username),
                        new XElement("PW", Password),
                        !String.IsNullOrEmpty(sessionData) ? new XElement("SD", sessionData) : null,
                        new XElement("MSGLST",
                            GetMessageElements(messages).ToList())));
        }

        private IEnumerable<XElement> GetMessageElements(IEnumerable<Message> messages)
        {
            var numInSession = 1;
            return messages.Select((msg) =>
            {
                msg.NumInSession = numInSession++;
                return new XElement("MSG", GetMessagePropertyElements(msg));
            });
        }

        private IEnumerable<XElement> GetMessagePropertyElements(Message msg)
        {
            var sms = msg as Sms;
            var mms = msg as Mms;

            yield return new XElement("ID", msg.NumInSession);
            yield return new XElement("TEXT", msg.Text);
            yield return new XElement("SND", msg.SenderNumber);
            yield return new XElement("RCV", msg.ReceiverNumber);

            if (!string.IsNullOrEmpty(msg.ShortCode))
                yield return new XElement("SHORTCODE", msg.ShortCode);

            if (msg.Tariff > 0)
                yield return new XElement("TARIFF", msg.Tariff);

            if (msg.RequestReceipt)
                yield return new XElement("RCPREQ", "Y");

            if (!string.IsNullOrEmpty(msg.CpaTag))
                yield return new XElement("CPATAG", msg.CpaTag);

            if (msg.Type.HasValue)
                yield return new XElement("OP", msg.Type.Value.ToString("D"));

            if (msg.TimeToLive.HasValue)
                yield return new XElement("TTL", msg.TimeToLive.Value.TotalMinutes.ToString("0"));

            if (msg.DeliveryTime.HasValue)
                yield return new XElement("DELIVERYTIME", msg.DeliveryTime.Value.ToString("yyyyMMddHHmm"));

            if (sms != null)
            {
                if (sms.Network != null)
                    yield return new XElement("NET", sms.Network.ToString());
                if (sms.AgeLimit.HasValue)
                    yield return new XElement("AGELIMIT", sms.AgeLimit.Value.ToString("0"));
                if (!string.IsNullOrEmpty(sms.ServiceCode))
                    yield return new XElement("SERVICECODE", sms.ServiceCode);
                if (sms.Replace.HasValue)
                    yield return new XElement("REPLACE", sms.Replace.Value.ToString("D"));
                if (sms != null && sms.IsFlashMessage)
                    yield return new XElement("CLASS", "0");
            }

            if (mms != null)
            {
                if (mms.MmsData != null && mms.MmsData.Length > 0)
                {
                    yield return new XElement("MMSFILE", System.Convert.ToBase64String(mms.MmsData));
                }
            }
        }

        protected static GatewayResponse GetSendResult(IEnumerable<Message> messages, TransportResult transportResult)
        {
            var result = new GatewayResponse();
            XElement logonElement = transportResult.Content.Descendants("LOGON").FirstOrDefault();
            XElement reasonElement = transportResult.Content.Descendants("REASON").FirstOrDefault();
            if (logonElement != null) result.Status = (BatchStatus)System.Enum.Parse(typeof(BatchStatus), logonElement.Value, ignoreCase: true);
            if (reasonElement != null) result.StatusText = reasonElement.Value;
            result.Results = ParseTransportResults(messages, transportResult);
            return result;
        }

        protected static IEnumerable<MessageResult> ParseTransportResults(IEnumerable<Message> messages, TransportResult transportResult)
        {
            return transportResult
                .Content
                .Descendants("MSG")
                .Select((el) =>
                {
                    var id = int.Parse(el.Element("ID").Value);
                    var message = messages.FirstOrDefault(m => m.NumInSession == id);
                    return new MessageResult
                    {
                        UserReference = message.UserReference,
                        Message = message,
                        GatewayReference = el.Element("REF") != null ? el.Element("REF").Value : null,
                        Status = GetMessageStatus(el),
                        StatusText = el.Element("INFO") != null ? el.Element("INFO").Value : null
                    };
                });
        }

        private static MessageStatus GetMessageStatus(XElement el)
        {
            MessageStatus status = MessageStatus.Fail;
            try
            {
                status = (MessageStatus)Enum.Parse(typeof(MessageStatus), el.Element("STATUS").Value, true);
            }
            catch
            {
                status = MessageStatus.Fail;
            }
            return status;
        }

        public string Password { get; set; }
        public string Username { get; set; }
        public int BatchSize { get; set; }
    }
}
