using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSWinCom.Gateway.Client
{
    public class NetworkSpecification
    {
        public NetworkSpecification(short mnc, short mcc)
        {
            Mnc = mnc;
            Mcc = mcc;
        }

        public short Mnc { get; set; }
        public short Mcc { get; set; }

        public override string ToString()
        {
            return String.Format("{0:000}:{1:00}", Mnc, Mcc);
        }

        public static implicit operator NetworkSpecification(string s)
        {
            var parts = s.Split(':');
            short mnc, mcc;

            if (parts.Length == 2 && short.TryParse(parts[0], out mnc) && short.TryParse(parts[1], out mcc))
                return new NetworkSpecification(mnc, mcc);
            else
                throw new ArgumentException("Not a valid network specification (mnc:mcc)");
        }
    }
}
