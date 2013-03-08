using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSWinCom.Gateway.Client
{
    public class Message
    {
        public string Text { get; set; }
        public string Recipient { get; set; }
        public string Sender { get; set; }
    }
}
