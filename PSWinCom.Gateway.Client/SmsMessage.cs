using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace PSWinCom.Gateway.Client
{
    /**
     * <summary>
     * Representation of an SMS message that can be sent to the PSWinCom Gateway XML API.
     * </summary>
     * <see cref=""/>
     * <seealso cref="PSWinCom.Gateway.Client.Message"/>
     */
    public class Sms : Message
    {
        public Sms()
            : base()
        {

        }

        public Sms(string from, string to, string text)
        {
            SenderNumber = from;
            ReceiverNumber = to;
            Text = text;
        }

        public string ServiceCode { get; set; }
        public int? AgeLimit { get; set; }
        public NetworkSpecification Network { get; set; }
        public bool IsFlashMessage { get; set; }
        public Replace? Replace { get; set; }
    }

    public enum Replace
    {
        Set1 = 1,
        Set2 = 2,
        Set3 = 3,
        Set4 = 4,
        Set5 = 5,
        Set6 = 6,
        Set7 = 7
    }

}
