using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace PSWinCom.Gateway.Client
{
    [XmlRoot("MSG")]
    public class Message
    {
        internal int NumInSession { get; set; }

        [XmlElement("TEXT")]
        public string Text { get; set; }
        [XmlElement("RCV")]
        public string ReceiverNumber { get; set; }
        [XmlElement("SND")]
        public string SenderNumber { get; set; }

        public int Tariff { get; set; }
        public bool RequestReceipt { get; set; }
        public NetworkSpecification Network { get; set; }
        public TimeSpan? TimeToLive { get; set; }
        public string CpaTag { get; set; }
        public int? AgeLimit { get; set; }
        public string ShortCode { get; set; }
        public string ServiceCode { get; set; }
        public DateTime? DeliveryTime { get; set; }
        public Replace? Replace { get; set; }
        public bool FlashMessage { get; set; }
        public MessageType? MessageType { get; set; }
        public byte[] MmsFile { get; set; }

        private string _useryReference;
        public string UserReference
        {
            get
            {
                if (String.IsNullOrEmpty(_useryReference))
                    return NumInSession.ToString();
                return _useryReference;
            }
            set
            {
                _useryReference = value;
            }
        }

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
        Unicode = 9
    }

}
