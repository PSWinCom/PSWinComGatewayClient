using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSWinCom.Gateway.Receiver
{
    public class IncomingMessage
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Text { get; set; }
    }
}
