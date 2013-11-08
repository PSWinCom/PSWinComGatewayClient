using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSWinCom.Gateway.Client
{
    public class MmsMessage : Message
    {
        public override MessageType? Type { get { return MessageType.MMSMessage; } set { if (value != MessageType.MMSMessage) throw new InvalidOperationException("Messagetype must be MmsMessage"); } }
        public byte[] MmsData { get; set; }
    }
}
