using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace PSWinCom.Gateway.Client
{
    public class Message : MessageBase
    {
        public string ShortCode { get; set; }
        public string ServiceCode { get; set; }
        public TimeSpan? TimeToLive { get; set; }
        public string CpaTag { get; set; }
        public int? AgeLimit { get; set; }
        public NetworkSpecification Network { get; set; }
        public bool FlashMessage { get; set; }
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

    public enum MessageType
    {
        Text = 1,
        Ringtone = 2,
        OperatorLogo = 3,
        CallerGroupGraphic = 4,
        Picture = 5,
        vCard = 6,
        vCalendar = 7,
        RawBinaryUDH = 8,
        Unicode = 9,
        WapPush = 10,
        OTABookmark = 11,
        OTASettings = 12,
        MMSMessage = 13
    }


    public class MmsMessage : MessageBase
    {
        public override MessageType? Type { get { return MessageType.MMSMessage; } set { throw new InvalidOperationException("Cannot set messagetype of MMS messages"); } }
        public byte[] MmsData { get; set; }
    }
}
