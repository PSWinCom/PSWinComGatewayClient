using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSWinCom.Gateway.Client
{
    public abstract class Message
    {
        public Message()
        {

        }

        private MessageType? _type;
        private string _useryReference;
        internal int NumInSession { get; set; }

        public string Text { get; set; }
        public string ReceiverNumber { get; set; }
        public string SenderNumber { get; set; }

        public string ShortCode { get; set; }
        public int Tariff { get; set; }
        public string CpaTag { get; set; }

        public bool RequestReceipt { get; set; }
        
        public TimeSpan? TimeToLive { get; set; }
        public DateTime? DeliveryTime { get; set; }

        public virtual MessageType? Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }

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
}
