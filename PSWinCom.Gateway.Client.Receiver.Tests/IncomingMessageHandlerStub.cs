using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PSWinCom.Gateway.Receiver;
using System.Xml.Linq;
using Should;

namespace PSWinCom.Gateway.Client.Receiver.Tests
{

    public class IncomingMessageHandlerStub : IncomingMessageHandler
    {
        private IncomingMessage _last_message;

        public XDocument Handle(XDocument document)
        {
            return HandleRequest(document);
        }

        public override bool OnIncomingMessageReceived(IncomingMessage message)
        {
            _last_message = message;
            return true;
        }
        public IncomingMessage LastMessage
        {
            get
            {
                return _last_message;
            }
        }
    }

}
