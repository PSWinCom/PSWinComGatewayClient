using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSWinCom.Gateway.Client
{
    public class Mms : Message
    {
        public Mms()
            : base()
        {

        }

        public Mms(string from, string to, string subject, byte[] mmsdata)
        {
            SenderNumber = from;
            ReceiverNumber = to;
            Text = subject;
            MmsData = mmsdata;
        }

        public override MessageType? Type { get { return MessageType.MMSMessage; } set { if (value != MessageType.MMSMessage) throw new InvalidOperationException("Messagetype must be MmsMessage"); } }
        public byte[] MmsData { get; set; }
    }
}
